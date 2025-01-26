using MagicOnion;
using MagicOnion.Server;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Server.Model;
using Server.Model.Context;
using Server.Model.Entity;
using Shared.Interfaces.Model.Entity;
using Shared.Interfaces.Services;
using System.Security.Cryptography.Xml;
using System.Xml.Linq;

namespace Server.Services
{
    public class FollowService : ServiceBase<IFollowService>, IFollowService
    {
        /// <summary>
        /// ユーザー取得API
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ReturnStatusException"></exception>
        public async UnaryResult<FollowingUser> ShowUserByNameAsync(string name)
        {
            // DB接続
            using var conn = new MySqlConnection(DbConnectionSettings.connectionBuilder.ConnectionString);
            await conn.OpenAsync();

            // SQL作成
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = $@"
            SELECT ut.id, ut.name, ut.character_id, rating
            FROM users AS ut
                     LEFT JOIN ratings ON ratings.id = ut.id
            where ut.name = '{name}';";

            // SQLクエリを発行し、結果を読み込む
            using MySqlDataReader reader = await command.ExecuteReaderAsync();
            if(!await reader.ReadAsync())
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "ユーザーを取得できませんでした。");
            }

            FollowingUser user = new FollowingUser();
            user.UserId = Convert.ToInt32(reader[0]);
            user.UserName = (string)reader[1];
            user.CharacterId = (int)reader[2];
            user.Rating = (int)reader[3];

            if (user == null)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "ユーザーを取得できませんでした。");
            }
            return user;
        }

        /// <summary>
        /// フォローしているユーザー一覧取得API
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async UnaryResult<FollowingUser[]> ShowFollowingUsersAsynk(int userId)
        {
            var mutualFollowerIds = await GetMutualFollowerIds(userId);
            var followingUsers = await GetFollowingUserAsync(userId);

            if (followingUsers.Count() <= 0)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "フォローしているユーザーが存在しません。");
            }

            FollowingUser[] result = new FollowingUser[followingUsers.Count()];
            for (int i = 0; i < followingUsers.Count(); i++)
            {
                result[i] = followingUsers[i];
                foreach(var id in mutualFollowerIds)
                {
                    if(id == result[i].UserId)
                    {
                        result[i].IsMutualFollow = true;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// フォローしているユーザー一覧を取得する
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        async Task<FollowingUser[]> GetFollowingUserAsync(int userId)
        {
            // DB接続
            using var conn = new MySqlConnection(DbConnectionSettings.connectionBuilder.ConnectionString);
            await conn.OpenAsync();

            // SQL作成
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = $@"
            SELECT ut.id, ut.name, ut.character_id, rating
            FROM users
                     LEFT JOIN follows ON users.id = follows.following_id
                     LEFT JOIN users AS ut ON ut.id = follows.Followee_id
                     LEFT JOIN ratings ON ratings.id = ut.id
            where following_id = {userId}
            ORDER BY ut.id;";

            // SQLクエリを発行し、結果を読み込む
            List<FollowingUser> followingUserList = new List<FollowingUser>();
            using MySqlDataReader reader = await command.ExecuteReaderAsync();

            while (reader.Read())
            {
                FollowingUser followingUser = new FollowingUser();
                followingUser.UserId = Convert.ToInt32(reader[0]);
                followingUser.UserName = (string)reader[1];
                followingUser.CharacterId = (int)reader[2];
                followingUser.Rating = (int)reader[3];
                followingUser.IsMutualFollow = false;
                followingUserList.Add(followingUser);
            }

            return followingUserList.ToArray();
        }

        /// <summary>
        /// 相互フォローのフォロワーのIDを取得
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        async Task<int[]> GetMutualFollowerIds(int userId)
        {
            // DB接続
            using var conn = new MySqlConnection(DbConnectionSettings.connectionBuilder.ConnectionString);
            await conn.OpenAsync();

            // SQL作成
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = $@"
            SELECT users.id
            FROM users
                     LEFT JOIN follows ON Followee_id = users.id
            WHERE follows.Followee_id IN ((SELECT following_id FROM follows WHERE Followee_id = {userId}))
              AND follows.following_id = {userId}
            ORDER BY users.id;";

            // SQLクエリを発行し、結果を読み込む
            List<int> followerList = new List<int>();
            using MySqlDataReader reader = await command.ExecuteReaderAsync();

            while (reader.Read())
            {
                int followerId = Convert.ToInt32(reader[0]);
                followerList.Add(followerId);
            }

            return followerList.ToArray();
        }

        /// <summary>
        /// フォロー登録API
        /// </summary>
        /// <param name="follow"></param>
        /// <returns></returns>
        public async UnaryResult RegistFollowAsync(int followingId, int followeeId)
        {
            using var context = new GameDbContext();

            // バリデーションチェック
            var users = context.Users.Where(x => x.Id == followingId || x.Id == followeeId).ToList();
            var followings = context.Follows.Where(x => x.Following_id == followingId).ToList();
            if (users.Count() < 2)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "ユーザーを取得できませんでした。");
            }
            else if(followings.Count >= ConstantManager.followingCntMax)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "これ以上はフォロー登録ができませんでした。");
            }
            foreach(var follow in followings)
            {
                if(follow.Followee_id == followeeId) 
                    throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "フォロー登録済みのユーザーです。");
            }

            var followData = new Follow();
            followData.Following_id = followingId;
            followData.Followee_id = followeeId;
            followData.Created_at = DateTime.Now;
            followData.Updated_at = DateTime.Now;
            context.Follows.Add(followData);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// フォロー解除API
        /// </summary>
        /// <param name="follow"></param>
        /// <returns></returns>
        public async UnaryResult RemoveFollowAsync(int followingId, int followeeId)
        {
            using var context = new GameDbContext();

            // バリデーションチェック
            var follow = context.Follows.Where(x => x.Following_id == followingId && x.Followee_id == followeeId).FirstOrDefault();
            if (follow == null)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "フォロー解除ができませんでした。");
            }

            context.Follows.Remove(follow);
            await context.SaveChangesAsync();
        }
    }
}

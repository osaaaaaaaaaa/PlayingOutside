using MagicOnion;
using MagicOnion.Server;
using MySqlConnector;
using Server.Model;
using Server.Model.Context;
using Server.Model.Entity;
using Shared.Interfaces.Model.Entity;
using Shared.Interfaces.Services;

namespace Server.Services
{
    public class RatingService : ServiceBase<IRatingService>, IRatingService
    {
        /// <summary>
        /// 全ユーザーを対象にしたランキング取得API
        /// </summary>
        /// <returns></returns>
        public async UnaryResult<RatingRanking[]> ShowGlobalRatingRanking()
        {
            // DB接続
            using var conn = new MySqlConnection(DbConnectionSettings.connectionBuilder.ConnectionString);
            await conn.OpenAsync();

            // SQL作成
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = $@"
            SELECT ut.name, ut.character_id, rt.rating
            FROM users AS ut
                     LEFT JOIN ratings AS rt ON ut.id = rt.user_id
            ORDER BY rt.rating DESC;";

            // SQLクエリを発行し、結果を読み込む
            List<RatingRanking> ratingRankingList = new List<RatingRanking>();
            using MySqlDataReader reader = await command.ExecuteReaderAsync();

            while (reader.Read())
            {
                RatingRanking ratingRanking = new RatingRanking();
                ratingRanking.UserName = (string)reader[0];
                ratingRanking.CharacterId = (int)reader[1];
                ratingRanking.Rating = (int)reader[2];
                ratingRankingList.Add(ratingRanking);
            }

            if(ratingRankingList.Count() == 0)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "ランキングデータを取得できませんでした。");
            }

            return ratingRankingList.ToArray();
        }

        /// <summary>
        /// フォローしているユーザーを対象にしたランキング取得API
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async UnaryResult<RatingRanking[]> ShowFollowedUsersRatingRanking(int userId)
        {
            // DB接続
            using var conn = new MySqlConnection(DbConnectionSettings.connectionBuilder.ConnectionString);
            await conn.OpenAsync();

            // SQL作成
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = $@"
            SELECT ut.name, ut.character_id, rt.rating
            FROM ratings AS rt
                     JOIN users AS ut ON rt.user_id = ut.id
                     JOIN (SELECT * FROM follows WHERE following_id = {userId}) AS ft ON ut.id = ft.followee_id
            ORDER BY rt.rating DESC;";

            // SQLクエリを発行し、結果を読み込む
            List<RatingRanking> ratingRankingList = new List<RatingRanking>();
            using MySqlDataReader reader = await command.ExecuteReaderAsync();

            while (reader.Read())
            {
                RatingRanking ratingRanking = new RatingRanking();
                ratingRanking.UserName = (string)reader[0];
                ratingRanking.CharacterId = (int)reader[1];
                ratingRanking.Rating = (int)reader[2];
                ratingRankingList.Add(ratingRanking);
            }

            if (ratingRankingList.Count() == 0)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "ランキングデータを取得できませんでした。");
            }

            return ratingRankingList.ToArray();
        }

        /// <summary>
        /// レーティング更新API
        /// </summary>
        /// <param name="rating"></param>
        /// <returns></returns>
        public async UnaryResult UpdateRatingAsync(int userId, int ratingDelta)
        {
            using var context = new GameDbContext();

            // バリデーションチェック
            var rating = context.Ratings.FirstOrDefault(rating => rating.user_id == userId);
            if (rating == null) 
            {
                // レーティングデータを登録する
                Rating registRating = new Rating();
                registRating.user_id = userId;
                registRating.rating = ratingDelta;
                registRating.Created_at = DateTime.Now;
                registRating.Updated_at = DateTime.Now;
                context.Ratings.Add(registRating);
                await context.SaveChangesAsync();
            }
            else
            {
                rating.rating = (rating.rating + ratingDelta) <= 0 ? 0 : rating.rating + ratingDelta;
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// レーティング取得API
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async UnaryResult<int> ShowRatingAsync(int userId)
        {
            using var context = new GameDbContext();

            // バリデーションチェック
            var rating = context.Ratings.FirstOrDefault(rating => rating.user_id == userId);
            if (rating == null) return 0;
            return rating.rating;
        }
    }
}

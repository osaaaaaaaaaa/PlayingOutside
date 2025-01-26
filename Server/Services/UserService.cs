using MagicOnion;
using MagicOnion.Server;
using Server.Model.Context;
using Server.Model.Entity;
using Shared.Interfaces.Model.Entity;
using Shared.Interfaces.Services;
using System.Xml.Linq;

namespace Server.Services
{
    public class UserService : ServiceBase<IUserService>,IUserService
    {
        /// <summary>
        /// ユーザー登録API
        /// </summary>
        /// <param name="name">ユーザー名</param>
        /// <returns></returns>
        /// <exception cref="ReturnStatusException"></exception>
        public async UnaryResult<User> RegistUserAsync(string name)
        {
            using var context = new GameDbContext();

            // バリデーションチェック
            if (context.Users.Where(user => user.Name == name).Count() > 0)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument,"※ 既に使用されている名前です");
            }
            else if(name == "")
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "※ 正しい名前を入力してください");
            }

            // テーブルにレコード追加
            User user = new User();
            user.Name = name;
            user.Token = "";
            user.Character_Id = 1;
            user.Created_at = DateTime.Now;
            user.Updated_at = DateTime.Now;
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// ユーザー取得API
        /// </summary>
        /// <param name="id">ユーザーID</param>
        /// <returns></returns>
        public async UnaryResult<User> ShowUserAsync(int id)
        {
            using var context = new GameDbContext();

            // バリデーションチェック
            var user = context.Users.FirstOrDefault(user => user.Id == id);
            if (user == null)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "ユーザーを取得できませんでした。");
            }
            return user;
        }

        /// <summary>
        /// ユーザー一覧取得API
        /// </summary>
        /// <returns></returns>
        public async UnaryResult<User[]> ShowAllUserAsync()
        {
            using var context = new GameDbContext();

            // バリデーションチェック
            var users = context.Users.ToArray();
            if (users.Count() <= 0)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "ユーザーを取得できませんでした。");
            }
            return users;
        }

        /// <summary>
        /// ユーザー情報更新API
        /// </summary>
        /// <param name="id">ユーザーID</param>
        /// <returns></returns>
        public async UnaryResult UpdateUserAsync(User request)
        {
            using var context = new GameDbContext();

            // バリデーションチェック
            var user = context.Users.FirstOrDefault(user => user.Id == request.Id);
            if (user == null)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "ユーザーを取得できませんでした。");
            }
            else if (request.Name != null && context.Users.Where(user => user.Name == request.Name).Count() > 0)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "使用できない名前です。");
            }
            else if(request.Character_Id != 0 && !Enum.IsDefined(typeof(EnumManager.Character_ID), request.Character_Id))
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "選択したキャラクターは存在しません。");
            }

            if(request.Name != null) user.Name = request.Name;
            if(request.Character_Id != 0) user.Character_Id = request.Character_Id;
            await context.SaveChangesAsync();
        }
    }
}

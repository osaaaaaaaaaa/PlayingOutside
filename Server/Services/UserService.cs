using MagicOnion;
using MagicOnion.Server;
using Server.Model.Context;
using Server.Model.Entity;
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
            Console.WriteLine("Received(RegistUserAsync):" + name);
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
            Console.WriteLine("Received(ShowUserAsync):" + id);
            using var context = new GameDbContext();

            // バリデーションチェック
            var user = context.Users.FirstOrDefault(user => user.Id == id);
            if (user == null)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "ユーザーを取得できません");
            }
            return user;
        }

        /// <summary>
        /// ユーザー一覧取得API
        /// </summary>
        /// <returns></returns>
        public async UnaryResult<User[]> ShowAllUserAsync()
        {
            Console.WriteLine("Received(ShowAllUserAsync)");
            using var context = new GameDbContext();

            // バリデーションチェック
            var users = context.Users.ToArray();
            if (users.Count() <= 0)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "ユーザーを取得できません");
            }
            return users;
        }

        /// <summary>
        /// ユーザー情報更新API
        /// </summary>
        /// <param name="id">ユーザーID</param>
        /// <returns></returns>
        public async UnaryResult<bool> UpdateUserAsync(User request)
        {
            Console.WriteLine("Received(UpdateUserAsync):" + request.Id);
            using var context = new GameDbContext();

            // バリデーションチェック
            var user = context.Users.FirstOrDefault(user => user.Id == request.Id);
            if (user == null)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "ユーザーを取得できません");
            }
            else if (context.Users.Where(user => user.Name == request.Name).Count() > 0)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "使用できない名前");
            }

            user.Name = request.Name;
            await context.SaveChangesAsync();
            return true;
        }
    }
}

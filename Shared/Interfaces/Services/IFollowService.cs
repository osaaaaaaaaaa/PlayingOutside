using MagicOnion;
using System;
using System.Collections.Generic;
using System.Text;
using Server.Model.Entity;

namespace Shared.Interfaces.Services
{
    public interface IFollowService : IService<IFollowService>
    {
        /// <summary>
        /// ユーザー取得API
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        UnaryResult<FollowingUser> ShowUserByNameAsync(string name);

        /// <summary>
        /// フォローしているユーザー一覧取得API
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        UnaryResult<FollowingUser[]> ShowFollowingUsersAsynk(int userId);

        /// <summary>
        /// フォロー登録API
        /// </summary>
        /// <param name="follow"></param>
        /// <returns></returns>
        UnaryResult RegistFollowAsync(int followingId, int followeeId);

        /// <summary>
        /// フォロー解除API
        /// </summary>
        /// <param name="follow"></param>
        /// <returns></returns>
        UnaryResult RemoveFollowAsync(int followingId, int followeeId);
    }
}

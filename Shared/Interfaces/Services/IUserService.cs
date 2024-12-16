using MagicOnion;
using System;
using System.Collections.Generic;
using System.Text;
using Server.Model.Entity;

namespace Shared.Interfaces.Services
{
    public interface IUserService : IService<IUserService>
    {
        /// <summary>
        /// ユーザー登録API
        /// </summary>
        /// <param name="name">ユーザー名</param>
        /// <returns></returns>
        UnaryResult<User> RegistUserAsync(string name);

        /// <summary>
        /// ユーザー取得API
        /// </summary>
        /// <param name="id">ユーザーID</param>
        /// <returns></returns>
        UnaryResult<User> ShowUserAsync(int id);

        /// <summary>
        /// ユーザー一覧取得API
        /// </summary>
        /// <returns></returns>
        UnaryResult<User[]> ShowAllUserAsync();

        /// <summary>
        /// ユーザー情報更新API
        /// </summary>
        /// <param name="request">リクエスト情報</param>
        /// <returns></returns>
        UnaryResult<bool> UpdateUserAsync(User request);
    }
}

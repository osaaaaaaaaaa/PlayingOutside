using MagicOnion;
using System;
using System.Collections.Generic;
using System.Text;
using Server.Model.Entity;

namespace Shared.Interfaces.Services
{
    public interface IRatingService : IService<IRatingService>
    {
        /// <summary>
        /// 全ユーザーを対象にしたランキング取得API
        /// </summary>
        /// <returns></returns>
        UnaryResult<RatingRanking[]> ShowGlobalRatingRanking();

        /// <summary>
        /// フォローしているユーザーを対象にしたランキング取得API
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        UnaryResult<RatingRanking[]> ShowFollowedUsersRatingRanking(int userId);

        /// <summary>
        /// レーティング更新API
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="ratingDelta"></param>
        /// <returns></returns>
        UnaryResult UpdateRatingAsync(int userId, int ratingDelta);

        /// <summary>
        /// レーティング取得API
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        UnaryResult<int> ShowRatingAsync(int userId);
    }
}

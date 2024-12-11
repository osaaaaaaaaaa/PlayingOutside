using MagicOnion;
using Shared.Interfaces.Model.Entity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces.StreamingHubs
{
    public interface IRoomHub : IStreamingHub<IRoomHub, IRoomHubReceiver>
    {
        //*********************************************************
        // クライアント側からサーバー側を呼び出す関数を定義する
        //*********************************************************

        /// <summary>
        /// ロビー入室
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns></returns>
        Task<JoinedUser[]> JoinLobbyAsynk(int userId);

        /// <summary>
        /// ユーザー入室
        /// </summary>
        /// <param name="roomName">ルーム名</param>
        /// <param name="userId">ユーザーID</param>
        /// <returns></returns>
        Task<JoinedUser[]> JoinAsynk(string roomName,int userId);

        /// <summary>
        /// ユーザー退室
        /// </summary>
        /// <returns></returns>
        Task LeaveAsynk();

        /// <summary>
        /// プレイヤー情報更新
        /// </summary>
        /// <param name="state">プレイヤー情報</param>
        /// <returns></returns>
        Task UpdatePlayerStateAsynk(PlayerState state);

        #region ゲーム開始までの処理
        /// <summary>
        /// 準備完了したかどうか
        /// </summary>
        /// <param name="isAllUsersReady">全員が準備完了できたかどうか</param>
        /// <returns></returns>
        Task OnReadyAsynk(bool isAllUsersReady);

        /// <summary>
        /// ゲーム開始前のカウントダウンが終了
        /// </summary>
        /// <returns></returns>
        Task OnCountdownOverAsynk();
        #endregion

        #region 競技『カントリーリレー』の処理
        /// <summary>
        /// エリアをクリアした処理
        /// </summary>
        /// <returns></returns>
        Task OnAreaClearedAsynk();

        /// <summary>
        /// 次のエリア(またはシーン)に移動する準備が完了した処理
        /// </summary>
        /// <returns></returns>
        Task OnReadyNextAreaAsynk(bool isLastArea);

        /// <summary>
        /// カウントダウン処理
        /// (マスタークライアントが繰り返し呼び出し)
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        Task OnCountDownAsynk(int currentTime);
        #endregion

        #region ゲーム終了までの処理(最終結果発表シーンの処理)

        /// <summary>
        /// 最終結果発表シーンに遷移した処理
        /// </summary>
        /// <returns></returns>
        Task OnTransitionFinalResultSceneAsynk();


        #endregion
    }
}

using Shared.Interfaces.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Interfaces.StreamingHubs
{
    /// <summary>
    /// サーバー側からクライアントを呼び出す関数を定義
    /// </summary>
    public interface IRoomHubReceiver
    {
        //*********************************************************
        // サーバー側からクライアント側を呼び出す関数を定義する
        //*********************************************************

        /// <summary>
        /// ユーザーの入室通知
        /// </summary>
        /// <param name="user">入室したユーザー情報</param>
        void OnJoin(JoinedUser user);

        /// <summary>
        /// ユーザーの退室通知
        /// </summary>
        /// <param name="connectionId">退室したユーザーの接続ID</param>
        void OnLeave(Guid connectionId);

        /// <summary>
        /// プレイヤーの情報更新通知
        /// </summary>
        /// <param name="connectionId">接続ID</param>
        /// <param name="state">プレイヤー情報</param>
        void OnUpdatePlayerState(Guid connectionId, PlayerState state);

        #region ゲーム開始までの処理
        /// <summary>
        /// 準備完了したかどうかの通知
        /// </summary>
        /// <param name="readyCnt">準備完了しているユーザー数</param>
        /// <param name="isTransitionGameScene">全員が準備完了したときにシーン遷移させる</param>
        void OnReady(int readyCnt, bool isTransitionGameScene);

        /// <summary>
        /// ゲーム開始通知
        /// </summary>
        void OnCountdownOver();
        #endregion

        #region 競技『カントリーリレー』の処理

        /// <summary>
        /// 現在のエリアをクリアした通知
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="userName"></param>
        void OnAreaCleared(Guid connectionId, string userName, bool isClearedAllUsers);

        /// <summary>
        /// 全員が次のエリアに移動する準備が完了した通知(ゲーム再開通知)
        /// </summary>
        /// <param name="restarningWaitSec">ゲーム再開までの待機時間(順位によって変動)</param>
        void OnReadyNextAreaAllUsers(float restarningWaitSec);
        #endregion

        #region ゲーム終了までの処理(最終結果発表シーンの処理)

        /// <summary>
        /// 最後の競技が終了した通知
        /// </summary>
        void OnAfterFinalGame();

        /// <summary>
        /// 全員が遷移できた通知
        /// </summary>
        void OnTransitionFinalResultSceneAllUsers(ResultData[] result);
        #endregion
    }
}

using Shared.Interfaces.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
        /// マッチング完了通知
        /// </summary>
        /// <param name="roomName">ルーム名</param>
        void OnMatching(string roomName);

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

        #region ゲーム共通処理
        /// <summary>
        /// カウントダウン開始通知
        /// (マスタークライアントが受信)
        /// </summary>
        void OnStartCountDown();

        /// <summary>
        /// カウントダウン通知
        /// </summary>
        /// <param name="currentTime"></param>
        void OnCountDown(int currentTime);

        /// <summary>
        /// 全員がゲーム終了処理を完了した通知
        /// </summary>
        void OnFinishGame(GameScene scene);

        /// <summary>
        /// コイン(ポイント)のドロップ通知
        /// </summary>
        /// <param name="startPoint">生成する位置</param>
        /// <param name="anglesY">コインの向き</param>
        /// <param name="coinNames">コインのユニークな名前</param>
        void OnDropCoins(Vector3 startPoint,int[] anglesY, string[] coinNames);

        /// <summary>
        /// 生成場所が異なるコイン(ポイント)のドロップ通知
        /// </summary>
        /// <param name="startPoins">生成する位置</param>
        /// <param name="coinNames">コインのユニークな名前</param>
        void OnDropCoinsAtRandomPositions(Vector3[] startPoins, string[] coinNames);

        /// <summary>
        /// アイテム入手通知
        /// </summary>
        void OnGetItem(Guid connectionId, string itemName, float option);
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

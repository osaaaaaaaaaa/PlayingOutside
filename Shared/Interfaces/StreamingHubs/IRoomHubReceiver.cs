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

        // ユーザーの入室通知
        void OnJoin(JoinedUser user);

        // ユーザーの退室通知
        void OnLeave(Guid connectionId);

        // プレイヤーの情報更新通知
        void OnUpdatePlayerState(Guid connectionId, PlayerState state);

        // 準備完了したかどうかの通知
        void OnReady(int readyCnt, bool isTransitionGameScene);

        // ゲーム開始通知
        void OnCountdownOver();
    }
}

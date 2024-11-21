using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using Grpc.Net.Client;
using MagicOnion.Client;
using Server.Model.Entity;
using Shared.Interfaces.Model.Entity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

public class RoomModel : BaseModel, IRoomHubReceiver
{
    private GrpcChannel channel;
    private IRoomHub roomHub;

    // 接続ID
    public Guid ConnectionId { get; private set; }
    // ユーザー接続通知
    public Action<JoinedUser> OnJoinedUser { get; set; }    // サーバーから通知が届いた際に、Action型に登録されている関数を呼び出す
    // ユーザー切断通知
    public Action<Guid> OnLeavedUser { get; set; }
    // プレイヤー情報更新通知
    public Action<Guid, PlayerState> OnUpdatePlayerStateUser { get; set; }

    /// <summary>
    /// 自分の状況
    /// </summary>
    public enum USER_STATE
    {
        disconnect = 0, // サーバーと未接続
        connect,        // サーバーと接続した
        joined,         // ルームに入室した
        leave,          // ルームから退室するリクエストを送信した
        leave_done,     // ルームからの退室が完了した
    }
    public USER_STATE userState { get; private set; } = USER_STATE.disconnect;

    /// <summary>
    /// MagicOnion接続処理
    /// </summary>
    public async UniTask ConnectAsync()
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler });
        roomHub = await StreamingHubClient.ConnectAsync<IRoomHub, IRoomHubReceiver>(channel, this);

        userState = USER_STATE.connect;
    }

    /// <summary>
    /// MagicOnion切断処理
    /// </summary>
    public async UniTask DisconnectAsync()
    {
        if(roomHub != null) await roomHub.DisposeAsync();
        if(channel != null) await channel.ShutdownAsync();
        roomHub = null;channel = null;

        userState = USER_STATE.disconnect;
    }

    /// <summary>
    /// // 破棄する際(アプリ終了時など)にサーバーとの接続を切断
    /// </summary>
    private async void OnDestroy()
    {
        if(userState == USER_STATE.joined) await LeaveAsync(); // 退出処理
        DisconnectAsync();
    }

    /// <summary>
    /// 入室処理
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async UniTask JoinAsync(string roomName, int userId)
    {
        JoinedUser[] users = await roomHub.JoinAsynk(roomName, userId);
        foreach (JoinedUser user in users)
        {
            if (user.UserData.Id == userId) this.ConnectionId = user.ConnectionId;  // 自身の接続IDを探して保存する
            OnJoinedUser(user); // アクションでモデルを使うクラスに通知
        }

        userState = USER_STATE.joined;
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// 他のユーザーが入室した通知
    /// </summary>
    /// <param name="user"></param>
    public void OnJoin(JoinedUser user)
    {
        // アクション実行
        if (userState == USER_STATE.joined) OnJoinedUser(user);
    }

    /// <summary>
    /// 退室処理
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async UniTask LeaveAsync()
    {
        userState = USER_STATE.leave;

        // サーバーに退室処理をリクエスト
        await roomHub.LeaveAsynk();
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// ユーザーが退室する通知（自分も含む）
    /// </summary>
    /// <param name="user"></param>
    public void OnLeave(Guid connectionId)
    {
        if (userState == USER_STATE.leave_done) return;
        // アクション実行
        OnLeavedUser(connectionId);

        // 自分が退室する場合
        if(this.ConnectionId == connectionId) userState = USER_STATE.leave_done;
    }

    /// <summary>
    /// プレイヤー情報更新処理
    /// </summary>
    /// <param name="playerState"></param>
    /// <returns></returns>
    public async UniTask UpdatePlayerStateAsync(PlayerState playerState)
    {
        // サーバーにプレイヤー情報更新処理をリクエスト
        await roomHub.UpdatePlayerStateAsynk(playerState);
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// プレイヤー情報更新通知
    /// </summary>
    /// <param name="user"></param>
    public void OnUpdatePlayerState(Guid connectionId, PlayerState playerState)
    {
        // アクション実行
        if (userState == USER_STATE.joined) OnUpdatePlayerStateUser(connectionId, playerState);
    }
}

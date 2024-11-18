using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using Grpc.Net.Client;
using MagicOnion.Client;
using Shared.Interfaces.Model.Entity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RoomModel : BaseModel, IRoomHubReceiver
{
    private GrpcChannel channel;
    private IRoomHub roomHub;

    // 接続ID
    public Guid ConnectionId { get; private set; }
    // ユーザー接続通知
    public Action<JoinedUser> OnJoinedUser { get; set; }   // サーバーから通知が届いた際に、Action型に登録されている関数を呼び出す

    /// <summary>
    /// MagicOnion接続処理
    /// </summary>
    public async UniTask ConnectAsync()
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler });
        roomHub = await StreamingHubClient.ConnectAsync<IRoomHub, IRoomHubReceiver>(channel, this);
    }

    /// <summary>
    /// MagicOnion切断処理
    /// </summary>
    public async UniTask DisconnectAsync()
    {
        if(roomHub != null) await roomHub.DisposeAsync();
        if(channel != null) await channel.ShutdownAsync();
        roomHub = null;channel = null;
    }

    private async void OnDestroy()
    {
        DisconnectAsync();  // 破棄する際(アプリ終了時など)にサーバーとの接続を切断
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
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]入室通知
    /// </summary>
    /// <param name="user"></param>
    public void OnJoin(JoinedUser user)
    {
        OnJoinedUser(user);
    }
}

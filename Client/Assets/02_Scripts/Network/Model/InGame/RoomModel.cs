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
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Playables;

public class RoomModel : BaseModel, IRoomHubReceiver
{
    private GrpcChannel channel;
    private IRoomHub roomHub;

    // 接続ID
    public Guid ConnectionId { get; private set; }
    // 接続するルーム名
    string connectionRoomName;
    public string ConnectionRoomName {  get { return connectionRoomName; } set { connectionRoomName = value; } }
    // DBから取得した自分のユーザー情報
    User myUserData;
    public User MyUserData { get { return myUserData; }set { myUserData = value; } }
    // 参加しているユーザーの情報
    public Dictionary<Guid,JoinedUser> JoinedUsers { get; private set; } = new Dictionary<Guid,JoinedUser>();

    #region サーバーから呼ばれるAction関数
    // ユーザー接続通知
    public Action<JoinedUser> OnJoinedUser { get; set; }    // サーバーから通知が届いた際に、Action型に登録されている関数を呼び出す
    // ユーザー切断通知
    public Action<Guid> OnLeavedUser { get; set; }
    // プレイヤー情報更新通知
    public Action<Guid, PlayerState> OnUpdatePlayerStateUser { get; set; }
    // 準備が完了したかどうかの通信
    public Action<int,bool> OnReadyUser { get; set; }
    // 全員がゲーム開始前のカウントダウン終了通知
    public Action OnCountdownOverAllUsers { get; set; }
    #endregion

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

    // インスタンス作成
    private static RoomModel instance;
    public static RoomModel Instance
    {
        get
        {
            // GETプロパティを呼ばれたときにインスタンスを作成する(初回のみ)
            if (instance == null)
            {
                GameObject gameObj = new GameObject("RoomModel");
                instance = gameObj.AddComponent<RoomModel>();
                DontDestroyOnLoad(gameObj);
            }
            return instance;
        }
    }

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
    /// 破棄する際(アプリ終了時など)にサーバーとの接続を切断
    /// </summary>
    private void OnDestroy()
    {
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
            JoinedUsers.Add(user.ConnectionId, user);
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
        JoinedUsers.Add(user.ConnectionId, user);
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
        if (userState != USER_STATE.joined) return;
        userState = USER_STATE.leave;
        JoinedUsers.Clear();

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
        JoinedUsers.Remove(connectionId);

        // 自分が退室する場合
        if (this.ConnectionId == connectionId)
        {
            userState = USER_STATE.leave_done;
            DisconnectAsync();
        }
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
        if (userState != USER_STATE.leave && userState != USER_STATE.leave_done) OnUpdatePlayerStateUser(connectionId, playerState);
    }

    /// <summary>
    /// 自分の準備が完了したかどうか
    /// </summary>
    /// <returns></returns>
    public async UniTask OnReadyAsynk(bool isReady)
    {

        // サーバーに準備が完了したかどうかをリクエスト
        await roomHub.OnReadyAsynk(isReady);
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// 準備完了したかどうかの通知
    /// </summary>
    public void OnReady(int readyCnt, bool isTransitionGameScene)
    {
        if (userState == USER_STATE.leave || userState == USER_STATE.leave_done) return;

        // アクション実行
        OnReadyUser(readyCnt, isTransitionGameScene);
    }

    /// <summary>
    /// 自分のゲーム開始前のカウントダウンが終了
    /// </summary>
    /// <returns></returns>
    public async UniTask OnCountdownOverAsynk()
    {
        // サーバーにカウントダウンが終了したことをリクエスト
        await roomHub.OnCountdownOverAsynk();
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// 全員がゲーム開始前のカウントダウン終了通知
    /// </summary>
    public void OnCountdownOver()
    {
        // アクション実行
        if (userState == USER_STATE.joined) OnCountdownOverAllUsers();
    }
}

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
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using static PlayerAnimatorController;

public class RoomModel : BaseModel, IRoomHubReceiver
{
    private GrpcChannel channel;
    private IRoomHub roomHub;

    // 接続ID
    public Guid ConnectionId { get; private set; }
    // 接続するルーム名
    string connectionRoomName;
    public string ConnectionRoomName {  get { return connectionRoomName; } set { connectionRoomName = value; } }
    // 参加しているユーザーの情報
    public Dictionary<Guid,JoinedUser> JoinedUsers { get; private set; } = new Dictionary<Guid,JoinedUser>();
    // マッチング中(マッチング完了)かどうか
    bool isMatchingRunning = false;
    public bool IsMatchingRunning { get { return isMatchingRunning; }  set { isMatchingRunning = value; } }

    #region サーバーから通知が届いた際に呼ばれるAction関数
    public Action OnmatchingUser { get; set; }
    // ユーザー接続通知
    public Action<JoinedUser> OnJoinedUser { get; set; }
    // ユーザー切断通知
    public Action<Guid> OnLeavedUser { get; set; }
    // プレイヤー情報更新通知
    public Action<Guid, PlayerState> OnUpdatePlayerStateUser { get; set; }
    // マスタークライアントの情報更新通知
    public Action<Guid, MasterClient> OnUpdateMasterClientUser { get; set; }

    #region ゲーム開始までの処理
    // カントリーリレーの中間エリアを選択した通知
    public Action<EnumManager.SELECT_MID_AREA_ID> OnSelectMidAreaIdUser { get; set; }
    // 準備が完了したかどうかの通信
    public Action<int,bool> OnReadyUser { get; set; }
    // 全員がゲーム開始前のカウントダウン終了通知
    public Action OnCountdownOverUser { get; set; }
    #endregion

    #region ゲーム共通の処理
    // ユーザーの所持ポイント更新通知
    public Action<Guid,int> OnUpdateScoreUser { get; set; }
    // カウントダウン通知
    public Action<int> OnCountDownUser { get; set; }
    // 全員のゲーム終了処理が完了した通知
    public Action<string> OnFinishGameUser { get; set; }
    // コインのドロップ通知
    public Action<Vector3, int[], string[], UserScore> OnDropCoinsUser { get; set; }
    // 生成場所が異なるコインのドロップ通知
    public Action<Vector3[], string[], UserScore> OnDropCoinsAtRandomPositionsUser { get; set; }
    // アイテム取得通知
    public Action<Guid, string,float> OnGetItemUser {  get; set; }
    // アイテム使用通知
    public Action<Guid,EnumManager.ITEM_ID> OnUseItemUser { get; set; }
    // アイテムの破棄通知
    public Action<string> OnDestroyItemUser { get; set; }
    // アイテムの生成通知
    public Action<Vector3,EnumManager.ITEM_ID,string> OnSpawnItemUser { get; set; }
    // 動的なオブジェクトの生成通知
    public Action<SpawnObject> OnSpawnObjectUser { get; set; }
    // 動物のギミック発動通知
    public Action<EnumManager.ANIMAL_GIMMICK_ID, string, Vector3[]> OnPlayAnimalGimmickUser { get; set; }
    #endregion

    #region 競技『カントリーリレー』の処理
    // 植物のギミックを破棄する通知
    public Action<string[]> OnDestroyPlantsGimmickUser { get; set; }
    // 植物のギミックを発動する通知
    public Action<string> OnTriggeringPlantGimmickUser { get; set; }
    // 現在のエリアをクリアした通知
    public Action<Guid,string,bool> OnAreaClearedUser { get; set; }
    // 全員が次のエリアに移動する準備が完了した通知 (ゲーム再開通知)
    public Action<float, EnumManager.RELAY_AREA_ID> OnReadyNextAreaUser { get; set; }
    // カウントダウン開始通知
    public Action OnStartCountDownUser { get; set; }
    #endregion

    #region ゲーム終了までの処理(最終結果発表シーンの処理)
    /// <summary>
    /// 最終結果発表シーンに遷移した処理
    /// </summary>
    public Action OnAfterFinalGameUser { get; set; }

    /// <summary>
    /// 全員が遷移できた通知
    /// </summary>
    public Action<ResultData[]> OnTransitionFinalResultSceneUser { get; set; }
    #endregion

    #endregion

    // バックグラウンドに移行したかどうか
    bool isBackground;

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
        userState = USER_STATE.disconnect;

        if (roomHub != null) await roomHub.DisposeAsync();
        if(channel != null) await channel.ShutdownAsync();
        roomHub = null;channel = null;
    }

    /// <summary>
    /// バックグラウンドに移行したらサーバーとの接続を切断
    /// </summary>
    /// <param name="pauseStatus"></param>
    private async void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Debug.Log("アプリが一時停止(バックグラウンドに行った)");
            if(userState == USER_STATE.joined)
            {
                isBackground = true;
            }
            await DisconnectAsync();
        }
        else
        {
            Debug.Log("アプリが再開(バックグラウンドから戻った)");
            if (isBackground)
            {
                isBackground = false;
                ErrorUIController.Instance.ShowErrorUI("サーバーとの接続が切断されました。", OnError);
            }
        }
    }

    /// <summary>
    /// 破棄する際(アプリ終了時など)にサーバーとの接続を切断
    /// </summary>
    private async void OnDestroy()
    {
        await DisconnectAsync();
    }

    void OnError()
    {
        if (SceneControler.Instance.isLoading) SceneManager.LoadScene("TopScene");
        else SceneControler.Instance.StartSceneLoad("TopScene");
    }

    /// <summary>
    /// ロビー入室処理
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async UniTask JoinLobbyAsynk(int userId)
    {
        JoinedUsers.Clear();
        JoinedUser[] users = await roomHub.JoinLobbyAsynk(userId);
        if(users != null) Debug.Log("ユーザー数" + users.Length);

        if (users == null)
        {
            await DisconnectAsync();

            // 入室に失敗した場合はTopSceneに戻る
            ErrorUIController.Instance.ShowErrorUI("入室に失敗しました。もう一度お試しください。", OnError);
        }
        else
        {
            foreach (JoinedUser user in users)
            {
                if (user.UserData.Id == userId) this.ConnectionId = user.ConnectionId;  // 自身の接続IDを探して保存する

                // 存在しなければ追加(複数のユーザーが同時に入室した際の対策)
                if (!JoinedUsers.ContainsKey(user.ConnectionId))
                {
                    JoinedUsers.Add(user.ConnectionId, user);
                    OnJoinedUser(user);
                }
            }

            userState = USER_STATE.joined;

            // マッチングが完了している場合
            if (JoinedUsers[this.ConnectionId].IsMatching)
            {
                Debug.Log("最後の人がマッチング完了");
                await LeaveAsync();
            }
        }
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// マッチングが完了した通知
    /// </summary>
    /// <param name="user"></param>
    public async void OnMatching(string roomName)
    {
        Debug.Log("マッチング完了通知");
        OnmatchingUser();
        ConnectionRoomName = roomName;
        if (userState == USER_STATE.joined) await LeaveAsync();  // 4番目のユーザー以外が処理するはず
    }

    /// <summary>
    /// 入室処理
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async UniTask JoinAsync(string roomName, int userId)
    {
        JoinedUsers.Clear();
        JoinedUser[] users = await roomHub.JoinAsynk(roomName, userId, IsMatchingRunning);

        if (users == null)
        {
            await DisconnectAsync();

            // 入室に失敗した場合はTopSceneに戻る
            ErrorUIController.Instance.ShowErrorUI("入室に失敗しました。もう一度お試しください。", OnError);
            return;
        }
        else
        {
            foreach (JoinedUser user in users)
            {
                if (user.UserData.Id == userId) this.ConnectionId = user.ConnectionId;  // 自身の接続IDを探して保存する

                // 存在しなければ追加(複数のユーザーが同時に入室した際の対策)
                if (!JoinedUsers.ContainsKey(user.ConnectionId))
                {
                    JoinedUsers.Add(user.ConnectionId, user);
                    OnJoinedUser(user);
                }
            }

            userState = USER_STATE.joined;

            // 自動マッチング時は入室できたら準備完了リクエストを送信
            if (IsMatchingRunning)
            {
                await ReadyAsynk(true);
            }
        }
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// 他のユーザーが入室した通知
    /// </summary>
    /// <param name="user"></param>
    public void OnJoin(JoinedUser user)
    {
        // アクション実行
        if (!JoinedUsers.ContainsKey(user.ConnectionId)) JoinedUsers.Add(user.ConnectionId, user);  // 存在しなければ追加
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

        // サーバーに退室処理をリクエスト
        await roomHub.LeaveAsynk();
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// ユーザーが退室する通知（自分も含む）
    /// </summary>
    /// <param name="user"></param>
    public void OnLeave(Guid connectionId, JoinedUser latestData)
    {
        if (userState == USER_STATE.leave_done) return;

        // 自分のユーザー情報を更新
        JoinedUsers[this.ConnectionId] = latestData;

        // アクション実行
        OnLeavedUser(connectionId);
        JoinedUsers.Remove(connectionId);

        // 自分が退室する場合
        if (this.ConnectionId == connectionId)
        {
            userState = USER_STATE.leave_done;
            if (!IsMatchingRunning) OnDestroy();    // 自動マッチング中以外であれば切断処理
        }
    }

    /// <summary>
    /// プレイヤー情報更新処理
    /// </summary>
    /// <param name="playerState"></param>
    /// <returns></returns>
    public async UniTask UpdatePlayerStateAsync(PlayerState playerState)
    {
        if (userState == USER_STATE.joined) 
        {
            await roomHub.UpdatePlayerStateAsynk(playerState);
        }
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// プレイヤー情報更新通知
    /// </summary>
    /// <param name="user"></param>
    public void OnUpdatePlayerState(Guid connectionId, PlayerState playerState)
    {
        // アクション実行
        if (userState == USER_STATE.joined) 
        {
            OnUpdatePlayerStateUser(connectionId, playerState); 
        }
    }

    /// <summary>
    /// マスタークライアントの情報更新処理
    /// </summary>
    /// <param name="playerState"></param>
    /// <returns></returns>
    public async UniTask UpdateMasterClientAsynk(MasterClient masterClient)
    {
        if (userState == USER_STATE.joined)
        {
            await roomHub.UpdateMasterClientAsynk(masterClient);
        }
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// マスタークライアントの情報更新通知
    /// </summary>
    /// <param name="user"></param>
    public void OnUpdateMasterClient(Guid connectionId, MasterClient masterClient)
    {
        // アクション実行
        if (userState == USER_STATE.joined)
        {
            OnUpdateMasterClientUser(connectionId, masterClient);
        }
    }

    #region ゲーム開始までの処理
    /// <summary>
    /// 競技カントリーリレーの中間エリアを選択する
    /// (マスタークライアントが処理)
    /// </summary>
    /// <param name="selectMidAreaId"></param>
    /// <returns></returns>
    public async UniTask SelectMidAreaAsynk(EnumManager.SELECT_MID_AREA_ID selectMidAreaId)
    {
        if(userState == USER_STATE.joined) await roomHub.SelectMidAreaAsynk(selectMidAreaId);
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// 競技カントリーリレーの中間エリアを選択した通知
    /// </summary>
    public void OnSelectMidArea(EnumManager.SELECT_MID_AREA_ID selectMidAreaId)
    {
        Debug.Log(selectMidAreaId.ToString());
        if (userState == USER_STATE.leave || userState == USER_STATE.leave_done) return;

        OnSelectMidAreaIdUser(selectMidAreaId);
    }

    /// <summary>
    /// 自分の準備が完了したかどうか
    /// </summary>
    /// <returns></returns>
    public async UniTask ReadyAsynk(bool isReady)
    {
        if(userState == USER_STATE.joined) await roomHub.ReadyAsynk(isReady);
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// 準備完了したかどうかの通知
    /// </summary>
    public void OnReady(int readyCnt, bool isTransitionGameScene)
    {
        Debug.Log(userState.ToString());
        if (userState == USER_STATE.leave || userState == USER_STATE.leave_done) return;

        // アクション実行
        OnReadyUser(readyCnt, isTransitionGameScene);
    }

    /// <summary>
    /// 自分のゲーム開始前のカウントダウンが終了
    /// </summary>
    /// <returns></returns>
    public async UniTask CountdownOverAsynk()
    {
        // サーバーにカウントダウンが終了したことをリクエスト
        await roomHub.CountdownOverAsynk();
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// 全員がゲーム開始前のカウントダウン終了通知
    /// </summary>
    public void OnCountdownOver()
    {
        // アクション実行
        if (userState == USER_STATE.joined) OnCountdownOverUser();
    }
    #endregion

    #region ゲーム共通処理
    /// <summary>
    /// ユーザーの所持ポイント更新通知
    /// </summary>
    public void OnUpdateScore(UserScore latestUserScore)
    {
        if (userState == USER_STATE.joined)
        {
            JoinedUsers[latestUserScore.ConnectionId].score = latestUserScore.LatestScore;
            OnUpdateScoreUser(latestUserScore.ConnectionId, latestUserScore.LatestScore);
        }
    }

    /// <summary>
    /// カウントダウン処理
    /// (マスタークライアントが処理)
    /// </summary>
    /// <param name="currentTime"></param>
    /// <returns></returns>
    public async UniTask CountDownAsynk(int currentTime)
    {
        if (userState == USER_STATE.joined) await roomHub.CountDownAsynk(currentTime);
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// カウントダウン通知
    /// </summary>
    /// <param name="restarningWaitSec"></param>
    public void OnCountDown(int currentTime)
    {
        if (userState == USER_STATE.joined) OnCountDownUser(currentTime);
    }

    /// <summary>
    /// ゲーム終了が完了したリクエスト
    /// </summary>
    /// <returns></returns>
    public async UniTask FinishGameAsynk()
    {
        if (userState == USER_STATE.joined) await roomHub.FinishGameAsynk();
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// 全員がゲーム終了処理を完了した通知
    /// </summary>
    /// <param name="scene"></param>
    public void OnFinishGame(EnumManager.SCENE_ID scene)
    {
        if (userState == USER_STATE.joined)
        {
            string sceneName = "";
            switch (scene)
            {
                case EnumManager.SCENE_ID.RelayGame:
                    sceneName = "RelayGameScene";
                    break;
                case EnumManager.SCENE_ID.FinalGame:
                    sceneName = "FinalGameScene";
                    break;
            }
            Debug.Log("次のゲーム：" + sceneName);
            OnFinishGameUser(sceneName);
        }
    }

    /// <summary>
    /// ノックダウン時に呼び出し
    /// </summary>
    /// <param name="startPoint"></param>
    /// <returns></returns>
    public async UniTask KnockDownAsynk(Vector3 startPoint)
    {
        if (userState == USER_STATE.joined) await roomHub.KnockDownAsynk(startPoint);
    }

    /// <summary>
    /// 場外に出た時に呼び出し
    /// </summary>
    /// <param name="rangePointA"></param>
    /// <param name="rangePointB"></param>
    /// <returns></returns>
    public async UniTask OutOfBoundsAsynk(Vector3 rangePointA, Vector3 rangePointB)
    {
        if (userState == USER_STATE.joined) await roomHub.OutOfBoundsAsynk(rangePointA, rangePointB);
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// コイン(ポイント)のドロップ通知
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="angleY"></param>
    public void OnDropCoins(Vector3 startPoint, int[] anglesY, string[] coinNames, UserScore latestUserScore)
    {
        if (userState == USER_STATE.joined)
        {
            JoinedUsers[latestUserScore.ConnectionId].score = latestUserScore.LatestScore;
            OnDropCoinsUser(startPoint, anglesY, coinNames, latestUserScore);
        }
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// 生成場所が異なるコイン(ポイント)のドロップ通知
    /// </summary>
    /// <param name="startPoins"></param>
    /// <param name="coinNames"></param>
    public void OnDropCoinsAtRandomPositions(Vector3[] startPoins, string[] coinNames, UserScore latestUserScore)
    {
        if (userState == USER_STATE.joined)
        {
            JoinedUsers[latestUserScore.ConnectionId].score = latestUserScore.LatestScore;
            OnDropCoinsAtRandomPositionsUser(startPoins, coinNames, latestUserScore);
        }
    }

    /// <summary>
    /// アイテムに触れた時に呼び出し
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="itemName"></param>
    /// <returns></returns>
    public async UniTask GetItemAsynk(EnumManager.ITEM_ID itemId, string itemName)
    {
        if (userState == USER_STATE.joined) await roomHub.GetItemAsynk(itemId, itemName);
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// アイテム取得通知
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemName"></param>
    /// <param name="option"></param>
    public void OnGetItem(Guid connectionId, string itemName, float option)
    {
        if (userState == USER_STATE.joined) OnGetItemUser(connectionId, itemName, option);
    }

    /// <summary>
    /// アイテム使用時に呼び出し
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async UniTask UseItemAsynk(EnumManager.ITEM_ID itemId)
    {
        if (userState == USER_STATE.joined) await roomHub.UseItemAsynk(itemId);
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// アイテム使用通知
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemId"></param>
    public void OnUseItem(Guid connectionId, EnumManager.ITEM_ID itemId)
    {
        if (userState == USER_STATE.joined) OnUseItemUser(connectionId, itemId);
    }

    /// <summary>
    /// アイテムの破棄
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async UniTask DestroyItemAsynk(string itemName)
    {
        if (userState == USER_STATE.joined) await roomHub.DestroyItemAsynk(itemName);
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// アイテムの破棄通知
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemId"></param>
    public void OnDestroyItem(string itemName)
    {
        if (userState == USER_STATE.joined) OnDestroyItemUser(itemName);
    }

    /// <summary>
    /// アイテムの生成
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async UniTask SpawnItemAsynk(Vector3 spawnPoint, EnumManager.ITEM_ID itemId)
    {
        if (userState == USER_STATE.joined) await roomHub.SpawnItemAsynk(spawnPoint, itemId);
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// アイテムの生成通知
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemId"></param>
    public void OnSpawnItem(Vector3 spawnPoint, EnumManager.ITEM_ID itemId, string itemName)
    {
        if (userState == USER_STATE.joined) OnSpawnItemUser(spawnPoint, itemId, itemName);
    }

    /// <summary>
    /// 動的なオブジェクトを生成
    /// </summary>
    /// <param name="spawnObject"></param>
    /// <returns></returns>
    public async UniTask SpawnObjectAsynk(SpawnObject spawnObject)
    {
        if (userState == USER_STATE.joined) await roomHub.SpawnObjectAsynk(spawnObject);
    }

    /// <summary>
    /// 動的なオブジェクトの生成通知
    /// </summary>
    /// <param name="spawnObject"></param>
    public void OnSpawnObject(SpawnObject spawnObject)
    {
        if (userState == USER_STATE.joined) OnSpawnObjectUser(spawnObject);
    }

    /// <summary>
    /// 動物のギミック発動処理
    /// </summary>
    /// <param name="animalName"></param>
    /// <param name="optionVec"></param>
    /// <returns></returns>
    public async UniTask PlayAnimalGimmickAsynk(EnumManager.ANIMAL_GIMMICK_ID animalId, string animalName, Vector3[] optionVec)
    {
        if (userState == USER_STATE.joined) await roomHub.PlayAnimalGimmickAsynk(animalId, animalName, optionVec);
    }

    /// <summary>
    /// 動物のギミック発動通知
    /// </summary>
    /// <param name="animalName"></param>
    /// <param name="optionVec"></param>
    public void OnPlayAnimalGimmick(EnumManager.ANIMAL_GIMMICK_ID animalId, string animalName, Vector3[] optionVec)
    {
        if (userState == USER_STATE.joined) OnPlayAnimalGimmickUser(animalId, animalName, optionVec);
    }
    #endregion

    #region 競技『カントリーリレー』の処理
    /// <summary>
    /// 植物のギミックを破棄するリクエスト
    /// (マスタークライアントが呼び出す)
    /// </summary>
    /// <param name="names"></param>
    /// <returns></returns>
    public async UniTask DestroyPlantsGimmickAsynk(string[] names)
    {
        if (userState == USER_STATE.joined) await this.roomHub.DestroyPlantsGimmickAsynk(names);
    }

    /// <summary>
    /// 植物のギミックを破棄する通知
    /// </summary>
    /// <param name="names"></param>
    public void OnDestroyPlantsGimmick(string[] names)
    {
        if (userState == USER_STATE.joined) OnDestroyPlantsGimmickUser(names);
    }

    /// <summary>
    /// 植物のギミックを発動するリクエスト
    /// </summary>
    /// <returns></returns>
    public async UniTask TriggeringPlantGimmickAsynk(string name)
    {
        if (userState == USER_STATE.joined) await this.roomHub.TriggeringPlantGimmickAsynk(name);
    }

    /// <summary>
    /// 植物のギミックを発動する通知
    /// </summary>
    /// <param name="name"></param>
    public void OnTriggeringPlantGimmick(string name)
    {
        if (userState == USER_STATE.joined) OnTriggeringPlantGimmickUser(name);
    }

    /// <summary>
    /// エリアをクリアした処理
    /// </summary>
    /// <param name="isLastArea"></param>
    /// <returns></returns>
    public async UniTask AreaClearedAsynk()
    {
        if (userState == USER_STATE.joined) await roomHub.AreaClearedAsynk();
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// 現在のエリアをクリアした通知
    /// </summary>
    public void OnAreaCleared(Guid connectionId, string userName, bool isClearedAllUsers)
    {
        if (userState == USER_STATE.joined) OnAreaClearedUser(connectionId, userName, isClearedAllUsers);
    }

    /// <summary>
    /// 次のエリアに移動する準備が完了した処理
    /// </summary>
    /// <returns></returns>
    public async UniTask ReadyNextAreaAsynk()
    {
        if (userState == USER_STATE.joined) await roomHub.ReadyNextAreaAsynk();
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// 全員が次のエリアに移動する準備が完了した通知 (ゲーム再開通知)
    /// </summary>
    /// <param name="restarningWaitSec"></param>
    public void OnReadyNextAreaAllUsers(float restarningWaitSec, EnumManager.RELAY_AREA_ID nextAreaId)
    {
        if (userState == USER_STATE.joined) OnReadyNextAreaUser(restarningWaitSec,nextAreaId);
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// (エリアクリア時などにマスタークライアントが受信)カウントダウン開始通知
    /// </summary>
    /// <param name="restarningWaitSec"></param>
    public void OnStartCountDown()
    {
        if (userState == USER_STATE.joined) OnStartCountDownUser();
    }
    #endregion


    #region ゲーム終了までの処理(最終結果発表シーンの処理)

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// 最後の競技が終了した通知
    /// </summary>
    public void OnAfterFinalGame()
    {
        if (userState == USER_STATE.joined) OnAfterFinalGameUser();
    }

    /// <summary>
    /// 最終結果発表シーンに遷移した処理
    /// </summary>
    /// <returns></returns>
    public async UniTask TransitionFinalResultSceneAsynk()
    {
        if (userState == USER_STATE.joined) await roomHub.TransitionFinalResultSceneAsynk();
    }

    /// <summary>
    /// [IRoomHubReceiverのインターフェイス]
    /// 全員が遷移できた通知
    /// </summary>
    /// <param name="result"></param>
    public async void OnTransitionFinalResultSceneAllUsers(ResultData[] result, int ratingDelta)
    {
        if (userState == USER_STATE.joined)
        {
            OnTransitionFinalResultSceneUser(result);

            // レーティング更新API
            await RatingModel.Instance.UpdateRatingAsync( UserModel.Instance.UserId, ratingDelta);
        }
    }
    #endregion
}

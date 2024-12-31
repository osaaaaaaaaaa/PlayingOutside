using Google.Protobuf.WellKnownTypes;
using MagicOnion.Server.Hubs;
using Server.Model.Context;
using Shared.Interfaces.Model.Entity;
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Intrinsics.X86;
using UnityEngine;
using static Shared.Interfaces.Model.Entity.EnumManager;

namespace Server.StreamingHubs
{

    public class RoomHub : StreamingHubBase<IRoomHub, IRoomHubReceiver>, IRoomHub
    {
        // どのルームに入っているか
        IGroup room;

        // 参加可能人数
        const int maxUsers = 4;
        // ゲーム開始可能人数
        const int minRequiredUsers = 2;
        // 加算するスコアのベース
        const int baseAddScore = 100;
        // 最大ゲーム数
        const int maxGameCnt = 2;
        // コイン１枚あたりのポイント数
        const float pointsPerCoin = 10;
        // コインをドロップするときの割合
        const float dropPointRate = 0.4f;

        /// <summary>
        /// ユーザーの切断処理
        /// </summary>
        /// <returns></returns>
        protected override ValueTask OnDisconnected()
        {
            Console.WriteLine("切断検知");

            // 入室した状態で切断した場合
            if (this.room != null 
                && this.room.GetInMemoryStorage<RoomData>() != null
                && this.room.GetInMemoryStorage<RoomData>().Get(this.ConnectionId) != null)
            {
                LeaveAsynk();
            }
            else
            {
                // 自分のデータを グループデータから削除する
                this.room.GetInMemoryStorage<RoomData>().Remove(this.ConnectionId);

                // ルーム内のメンバーから削除
                room.RemoveAsync(this.Context);
            }

            return CompletedTask;
        }

        /// <summary>
        /// マッチング処理
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<JoinedUser[]> JoinLobbyAsynk(int userId)
        {
            // ロビーに参加
            JoinedUser[] joinedUsers = await JoinAsynk("Lobby", userId, false);
            if (joinedUsers == null) return null;

            var roomStorage = this.room.GetInMemoryStorage<RoomData>();
            lock (roomStorage)
            {
                foreach (var user in joinedUsers)
                {
                    // ルーム参加者がマッチング完了済みの場合は退室する
                    if (user.IsMatching)
                    {
                        Console.WriteLine("Lobby => nullを返す");

                        // 自分のデータを グループデータから削除する
                        roomStorage.Remove(this.ConnectionId);
                        // ルーム内のメンバーから削除
                        room.RemoveAsync(this.Context);
                        return null;
                    }
                }

                // 人数が集まったかチェック
                if (joinedUsers.Length == maxUsers)
                {
                    foreach (var user in joinedUsers)
                    {
                        user.IsMatching = true;
                    }

                    var guid = Guid.NewGuid();
                    this.Broadcast(room).OnMatching(guid.ToString());
                }

                return joinedUsers;
            }
        }

        /// <summary>
        /// 入室処理
        /// </summary>
        /// <param name="roomName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<JoinedUser[]> JoinAsynk(string roomName, int userId, bool isMatching)
        {
            // 指定したルームに参加、ルームを保持
            this.room = await this.Group.AddAsync(roomName);

            // ストレージには一種類の型しか使えないため、他の情報を入れたい場合は、RoomDataクラスに追加
            var roomStorage = this.room.GetInMemoryStorage<RoomData>();

            // [排他制御] 同時に入室した際に、人数制限のチェックや入室順の取得時におかしくなる可能性があるため
            lock (roomStorage)
            {
                // そのルームではゲーム中かどうかチェック
                bool isFailed = false;
                foreach(var storageData in roomStorage.AllValues.ToArray<RoomData>())
                {
                    if(storageData.JoinedUser.IsGameRunning)
                    {
                        isFailed = true;
                        Console.WriteLine("ゲーム中のため参加できません");
                        break;
                    }
                }

                // 人数制限もチェック
                if (isFailed || roomStorage.AllValues.ToArray<RoomData>().Length >= maxUsers)
                {
                    // ルーム内のメンバーから削除
                    room.RemoveAsync(this.Context);
                    Console.WriteLine("nullを返す");
                    return null;
                }

                // DBからユーザー情報取得
                GameDbContext context = new GameDbContext();
                var user = context.Users.Where(user => user.Id == userId).FirstOrDefault();

                // グループストレージにユーザーデータを格納
                int joinOrder = GetJoinOrder(roomStorage.AllValues.ToArray<RoomData>());
                var joinedUser = new JoinedUser() 
                { 
                    ConnectionId = this.ConnectionId, 
                    UserData = user, 
                    JoinOrder = joinOrder, 
                    IsMasterClient = (roomStorage.AllValues.ToArray<RoomData>().Length == 0), 
                    IsStartMasterCountDown = false, 
                    IsGameRunning = false,
                    IsMatching = isMatching,
                    score = 0,
                };
                var roomData = new RoomData() { JoinedUser = joinedUser, PlayerState = new PlayerState(), UserState = new UserState() };
                roomStorage.Set(this.ConnectionId, roomData);    // 自動で割り当てされるユーザーごとの接続IDに紐づけて保存したいデータを格納する

                // 自分以外のルーム参加者全員に、ユーザーの入室通知を送信(Broodcast:配布する,Except:自分以外)
                // ※Broadcast(room) で自身も含めて関数を実行できる
                this.BroadcastExceptSelf(room).OnJoin(joinedUser);

                // ルームデータ(グループストレージ内のデータ)情報取得
                RoomData[] roomDataList = roomStorage.AllValues.ToArray<RoomData>();

                // 参加中のユーザー情報を返す
                JoinedUser[] joinedUserList = new JoinedUser[roomDataList.Length];
                for (int i = 0; i < joinedUserList.Length; i++)
                {
                    joinedUserList[i] = roomDataList[i].JoinedUser;
                }

                Console.WriteLine(roomData.JoinedUser.ConnectionId + "：" + roomData.JoinedUser.UserData.Name + "が入室" + "...IsMasterClient:" + roomData.JoinedUser.IsMasterClient);

                return joinedUserList;
            }
        }

        /// <summary>
        /// 入室順を取得する
        /// </summary>
        int GetJoinOrder(RoomData[] roomData)
        {
            int joinOrder = 1;

            int roopCnt = 0;
            while (roopCnt < roomData.Length)
            {
                roopCnt = 0;
                for (int i = roomData.Length - 1; i >= 0; i--, roopCnt++)
                {
                    if (roomData[i].JoinedUser.JoinOrder == joinOrder)
                    {
                        joinOrder++;
                        break;
                    }
                }
            }

            return joinOrder;
        }

        /// <summary>
        /// 退室処理
        /// </summary>
        /// <returns></returns>
        public async Task LeaveAsynk()
        {
            var roomStorage = room.GetInMemoryStorage<RoomData>();
            var user = roomStorage.Get(this.ConnectionId).JoinedUser;
            var dataList = roomStorage.AllValues.ToArray<RoomData>();
            Console.WriteLine(user.UserData.Name + "が退室しました");

            // 自分がマスタークライアントの場合は、新しくマスタークライアントを選ぶ
            lock (roomStorage)
            {
                if (user.IsMasterClient) AssignNewMasterClient(dataList, user.ConnectionId);
            }

            foreach (var latestData in dataList)
            {
                // ルーム参加者にユーザーの退室通知を送信
                this.BroadcastTo(this.room, latestData.JoinedUser.ConnectionId).OnLeave(this.ConnectionId, latestData.JoinedUser);
            }

            // 自分のデータを グループデータから削除する
            this.room.GetInMemoryStorage<RoomData>().Remove(this.ConnectionId);
            // ルーム内のメンバーから自分を削除
            await room.RemoveAsync(this.Context);
        }

        /// <summary>
        /// 新しくマスタークライアントを任命する
        /// </summary>
        void AssignNewMasterClient(RoomData[] roomDatas, Guid exclusionId)
        {
            Guid connectionId = Guid.Empty;
            foreach (RoomData roomData in roomDatas)
            {
                if (roomData.JoinedUser.ConnectionId == exclusionId)
                {
                    roomData.JoinedUser.IsMasterClient = false;
                }
                else if (!roomData.JoinedUser.IsMasterClient)
                {
                    connectionId = roomData.JoinedUser.ConnectionId;
                    roomData.JoinedUser.IsMasterClient = true;
                    break;
                }
            }
        }

        /// <summary>
        /// マスタークライアント取得処理
        /// </summary>
        /// <returns></returns>
        RoomData GetMasterClient(RoomData[] roomDataList)
        {
            var roomStorage = room.GetInMemoryStorage<RoomData>();
            RoomData master = null;

            foreach (var roomData in roomDataList)
            {
                if (roomData.JoinedUser.IsMasterClient)
                {
                    master = roomData;
                    break;
                }
            }
            return master;
        }

        /// <summary>
        /// プレイヤー情報更新
        /// </summary>
        /// <returns></returns>
        public async Task UpdatePlayerStateAsynk(PlayerState state)
        {
            var roomStorage = room.GetInMemoryStorage<RoomData>();

            // ストレージ内のプレイヤー情報を更新する
            var data = roomStorage.Get(this.ConnectionId);
            data.PlayerState = state;

            // ルーム参加者にプレイヤー情報更新通知を送信
            this.BroadcastExceptSelf(room).OnUpdatePlayerState(this.ConnectionId, state);
        }

        /// <summary>
        /// マスタークライアントの情報更新
        /// </summary>
        /// <param name="masterClient"></param>
        /// <returns></returns>
        public async Task UpdateMasterClientAsynk(MasterClient masterClient)
        {
            var roomStorage = room.GetInMemoryStorage<RoomData>();

            // ストレージ内のプレイヤー情報を更新する
            var data = roomStorage.Get(this.ConnectionId);
            data.PlayerState = masterClient.playerState;

            // ルーム参加者にマスタークライアントの情報更新通知を送信
            this.BroadcastExceptSelf(room).OnUpdateMasterClient(this.ConnectionId, masterClient);
        }

        /// <summary>
        /// 準備完了したかどうか
        /// </summary>
        /// <returns></returns>
        public async Task ReadyAsynk(bool isReady)
        {
            bool isAllUsersReady = false;
            int readyCnt = 0;

            var roomStorage = room.GetInMemoryStorage<RoomData>();

            Console.WriteLine(roomStorage.Get(this.ConnectionId).JoinedUser.UserData.Name + "、完了リクエスト受信(lockの手前)");
            // [排他制御] 準備完了チェックが複数同時に処理すると、データの整合性に異常がでるため
            lock (roomStorage)
            {
                // 送信したユーザーのデータを更新
                var data = roomStorage.Get(this.ConnectionId);
                data.UserState.isReadyRoom = isReady;

                if (data.JoinedUser.IsGameRunning) return;

                // 全員が準備完了したかどうかチェック
                bool isMatching = false;
                RoomData[] roomDataList = roomStorage.AllValues.ToArray<RoomData>();
                foreach (var roomData in roomDataList)
                {
                    if (roomData.UserState.isReadyRoom) readyCnt++;
                    if (roomData.JoinedUser.IsMatching) isMatching = true;
                }
                Console.WriteLine(roomStorage.Get(this.ConnectionId).JoinedUser.UserData.Name + "、完了リクエスト受信(" + readyCnt + ")");

                // ゲームに参加する人数を取得 [マッチングからゲームに参加している場合:maxUsers]
                int maxRequiredUsers = isMatching ? maxUsers : roomDataList.Length;

                // 最低人数以上かつ全員が準備完了している場合
                if (roomDataList.Length >= minRequiredUsers && readyCnt == maxRequiredUsers)
                {
                    foreach (var roomData in roomDataList)
                    {
                        roomData.JoinedUser.IsGameRunning = true;
                    }

                    isAllUsersReady = true;
                    Console.WriteLine("全員が準備完了した(" + readyCnt + ")");
                }

                // 準備完了通知
                this.Broadcast(room).OnReady(readyCnt, isAllUsersReady);
            }
        }

        /// <summary>
        /// ゲーム開始前のカウントダウン終了処理
        /// </summary>
        /// <returns></returns>
        public async Task CountdownOverAsynk()
        {
            int readyCnt = 0;
            RoomData[] roomDataList;

            // 送信したユーザーのデータを更新
            var roomStorage = room.GetInMemoryStorage<RoomData>();

            // [排他制御] カウントダウンの終了チェックが複数同時に処理すると、データの整合性に異常がでるため
            lock (roomStorage)
            {
                var data = roomStorage.Get(this.ConnectionId);
                data.UserState.isCountdownOver = true;

                // 全員がカウントダウン終了したかどうかチェック
                roomDataList = roomStorage.AllValues.ToArray<RoomData>();
                foreach (var roomData in roomDataList)
                {
                    if (roomData.UserState.isCountdownOver) readyCnt++;
                }

                // ゲーム開始通知を配る
                if (readyCnt == roomDataList.Length) this.Broadcast(room).OnCountdownOver();

                // マスタークライアント##############################################################################
                
                // 最終競技が始まる場合
                if(data.UserState.FinishGameCnt == maxGameCnt - 1)
                {
                    // マスタークライアントにカウントダウン開始通知
                    var master = GetMasterClient(roomDataList);
                    if (master != null && !master.JoinedUser.IsStartMasterCountDown)
                    {
                        master.JoinedUser.IsStartMasterCountDown = true;
                        this.BroadcastTo(room, master.JoinedUser.ConnectionId).OnStartCountDown();
                        Console.WriteLine("最終競技でカウントダウン開始通知");
                    }
                }
            }
        }

        /// <summary>
        /// 各自の画面でゲームが終了
        /// </summary>
        /// <returns></returns>
        public async Task FinishGameAsynk()
        {
            var roomStorage = room.GetInMemoryStorage<RoomData>();
            lock (roomStorage)
            {
                var dataSelf = roomStorage.Get(this.ConnectionId);
                dataSelf.UserState.isFinishGame = true;
                Console.WriteLine(dataSelf.JoinedUser.UserData.Name + "がゲーム終了を迎えた");

                // 全員がゲーム終了したかどうかチェック
                RoomData[] roomDataList = roomStorage.AllValues.ToArray<RoomData>();
                foreach (var roomData in roomDataList)
                {
                    if (!roomData.UserState.isFinishGame) return;
                }

                foreach (var roomData in roomDataList)
                {
                    roomData.JoinedUser.IsStartMasterCountDown = false;
                    roomData.UserState.isCountdownOver = false;
                    roomData.UserState.isFinishGame = false;
                    roomData.UserState.FinishGameCnt++;
                    roomData.UserState.usedItemNameList.Clear();
                }

                Console.WriteLine("現在の終了したゲーム数：" + dataSelf.UserState.FinishGameCnt);

                // 現在の競技が最終競技だった場合
                if (dataSelf.UserState.FinishGameCnt == maxGameCnt)
                {
                    // 全ての競技が終了した通知を配る
                    this.Broadcast(room).OnAfterFinalGame();
                    Console.WriteLine("最終結果発表シーン");
                }
                else
                {
                    EnumManager.SCENE_ID gameScene = EnumManager.SCENE_ID.FinalGame;
                    // 次の競技が最終競技になる場合
                    if (dataSelf.UserState.FinishGameCnt == maxGameCnt - 1)
                    {
                        gameScene = EnumManager.SCENE_ID.FinalGame;
                    }
                    else
                    {
                        // 次の競技を抽選する
                    }

                    // 全員がゲーム終了処理を完了した通知を配る
                    this.Broadcast(room).OnFinishGame(gameScene);
                }
            }
        }

        /// <summary>
        /// ノックダウン時の処理
        /// </summary>
        /// <returns></returns>
        public async Task KnockDownAsynk(Vector3 startPoint)
        {
            var roomStorage = room.GetInMemoryStorage<RoomData>();
            lock (roomStorage)
            {
                var dataSelf = roomStorage.Get(this.ConnectionId);
                //Console.WriteLine(dataSelf.JoinedUser.UserData.Name + "の所持ポイント：" + dataSelf.UserState.score);

                // コインをドロップできる分の所持ポイントがある場合
                if (dataSelf.JoinedUser.score >= pointsPerCoin)
                {
                    float dropCoinCnt = GetDropCoinCnt(dataSelf);

                    // 所持ポイントを減算
                    dataSelf.JoinedUser.score -= (int)(dropCoinCnt * pointsPerCoin);

                    int[] coinAnglesY = new int[(int)dropCoinCnt];
                    string[] coinNames = new string[(int)dropCoinCnt];
                    for (int i = 0; i < coinAnglesY.Length; i++)
                    {
                        var rnd = new Random();
                        coinAnglesY[i] = rnd.Next(0, 360);
                        coinNames[i] = Guid.NewGuid().ToString();
                    }

                    var userScore = new UserScore()
                    {
                        ConnectionId = this.ConnectionId,
                        LatestScore = dataSelf.JoinedUser.score
                    };

                    // コインのドロップ通知を配る
                    this.Broadcast(this.room).OnDropCoins(startPoint, coinAnglesY, coinNames, userScore);
                }
            }
        }

        /// <summary>
        /// 場外に出た時
        /// </summary>
        /// <param name="rangePointA">ステージの範囲A</param>
        /// <param name="rangePointB">ステージの範囲B</param>
        /// <returns></returns>
        public async Task OutOfBoundsAsynk(Vector3 rangePointA, Vector3 rangePointB)
        {
            var roomStorage = room.GetInMemoryStorage<RoomData>();
            lock (roomStorage)
            {
                var dataSelf = roomStorage.Get(this.ConnectionId);

                // コインをドロップできる分の所持ポイントがある場合
                if (dataSelf.JoinedUser.score >= pointsPerCoin)
                {
                    float dropCoinCnt = GetDropCoinCnt(dataSelf);

                    // 所持ポイントを減算
                    dataSelf.JoinedUser.score -= (int)(dropCoinCnt * pointsPerCoin);

                    Vector3[] startPoints = new Vector3[(int)dropCoinCnt];
                    int[] coinAnglesY = new int[(int)dropCoinCnt];
                    string[] coinNames = new string[(int)dropCoinCnt];
                    for (int i = 0; i < coinAnglesY.Length; i++)
                    {
                        var rnd = new Random();

                        float x = rnd.Next((int)rangePointA.x, (int)rangePointB.x);
                        float y = rnd.Next((int)rangePointA.y, (int)rangePointB.y);
                        float z = rnd.Next((int)rangePointA.z, (int)rangePointB.z);

                        startPoints[i] = new Vector3(x, y, z);
                        coinAnglesY[i] = rnd.Next(0, 360);
                        coinNames[i] = Guid.NewGuid().ToString();
                    }

                    var userScore = new UserScore()
                    {
                        ConnectionId = this.ConnectionId,
                        LatestScore = dataSelf.JoinedUser.score
                    };

                    // 生成場所が異なるコインのドロップ通知を配る
                    this.Broadcast(this.room).OnDropCoinsAtRandomPositions(startPoints, coinNames, userScore);
                }
            }
        }

        /// <summary>
        /// ユーザーがドロップするコインの枚数を取得する
        /// </summary>
        /// <returns></returns>
        float GetDropCoinCnt(RoomData dataSelf)
        {
            float coins = MathF.Floor(dataSelf.JoinedUser.score / pointsPerCoin); // 所持するコインの枚数
            float dropCoinCnt = MathF.Floor(coins * dropPointRate);    // ドロップするコインの枚数(40%ドロップ)
            if (dropCoinCnt <= 0) dropCoinCnt = 1;  // 調整用
            return dropCoinCnt;
        }

        /// <summary>
        /// アイテムの取得時の処理
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public async Task GetItemAsynk(EnumManager.ITEM_ID itemId, string itemName)
        {
            var roomStorage = room.GetInMemoryStorage<RoomData>();
            lock (roomStorage)
            {
                // 他のユーザーが既に同じアイテム名を使用していないかチェック
                RoomData[] roomDataList = roomStorage.AllValues.ToArray<RoomData>();
                foreach (RoomData roomData in roomDataList)
                {
                    foreach (var name in roomData.UserState.usedItemNameList)
                    {
                        if (name == itemName)
                        {
                            return;
                        }
                    }
                }

                // アイテムを入手する
                var dataSelf = roomStorage.Get(this.ConnectionId);
                dataSelf.UserState.usedItemNameList.Add(itemName);

                float option = 0;
                switch (itemId)
                {
                    case EnumManager.ITEM_ID.Coin:
                        dataSelf.JoinedUser.score += (int)pointsPerCoin;
                        option = dataSelf.JoinedUser.score;
                        break;
                    default:
                        break;
                }

                this.Broadcast(this.room).OnGetItem(this.ConnectionId, itemName, option);
            }
        }

        /// <summary>
        /// アイテムの使用
        /// </summary>
        /// <param name="ConnectionId"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public async Task UseItemAsynk(EnumManager.ITEM_ID itemId)
        {
            this.Broadcast(this.room).OnUseItem(this.ConnectionId, itemId);
        }

        /// <summary>
        /// アイテムの破棄
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public async Task DestroyItemAsynk(string itemName)
        {
            var roomStorage = room.GetInMemoryStorage<RoomData>();
            lock (roomStorage) 
            {
                // 他のユーザーが既に同じアイテム名を使用していないかチェック
                RoomData[] roomDataList = roomStorage.AllValues.ToArray<RoomData>();
                foreach (RoomData roomData in roomDataList)
                {
                    foreach (var name in roomData.UserState.usedItemNameList)
                    {
                        if (name == itemName)
                        {
                            return;
                        }
                    }
                }

                // マスターがアイテムを使用したことにする
                var dataSelf = roomStorage.Get(this.ConnectionId);
                dataSelf.UserState.usedItemNameList.Add(itemName);
            }

            this.Broadcast(this.room).OnDestroyItem(itemName);
        }

        /// <summary>
        /// アイテムの生成
        /// </summary>
        /// <param name="spawnPoint"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public async Task SpawnItemAsynk(Vector3 spawnPoint, EnumManager.ITEM_ID itemId)
        {
            this.Broadcast(this.room).OnSpawnItem(spawnPoint, itemId, Guid.NewGuid().ToString());
        }

        /// <summary>
        /// エリアをクリアした処理
        /// </summary>
        /// <returns></returns>
        public async Task AreaClearedAsynk()
        {
            var roomStorage = room.GetInMemoryStorage<RoomData>();

            // [排他制御] カウントダウンの終了チェックが複数同時に処理すると、データの整合性に異常がでるため
            lock (roomStorage)
            {
                RoomData[] roomDataList = roomStorage.AllValues.ToArray<RoomData>();
                var dataSelf = roomStorage.Get(this.ConnectionId);
                if (dataSelf.UserState.isAreaCleared) return;

                // 送信したユーザーのデータを更新
                dataSelf.UserState.isAreaCleared = true;
                dataSelf.UserState.areaGoalRank = GetAreaClearRank(roomDataList);
                dataSelf.JoinedUser.score += baseAddScore / dataSelf.UserState.areaGoalRank;

                RoomData master = GetMasterClient(roomDataList);
                if (master != null && !master.JoinedUser.IsStartMasterCountDown) 
                {
                    master.JoinedUser.IsStartMasterCountDown = true;

                    // マスタークライアントにカウントダウン開始通知を配る
                    this.BroadcastTo(room, master.JoinedUser.ConnectionId).OnStartCountDown();
                }

                // 全員が現在のエリアをクリアしたかチェック
                int readyCnt = 0;
                foreach (var roomData in roomDataList)
                {
                    if (roomData.UserState.isAreaCleared) readyCnt++;
                }

                // エリアのクリア通知を自分以外に配る
                this.BroadcastExceptSelf(room).OnAreaCleared(this.ConnectionId, dataSelf.JoinedUser.UserData.Name, (readyCnt == roomDataList.Length));

                // ユーザーのスコアを更新する
                var userScore = new UserScore()
                {
                    ConnectionId = this.ConnectionId,
                    LatestScore = dataSelf.JoinedUser.score
                };
                this.Broadcast(this.room).OnUpdateScore(userScore);
            }
        }

        /// <summary>
        /// エリアをクリアしたときの順位を取得
        /// </summary>
        /// <param name="roomData"></param>
        /// <returns></returns>
        int GetAreaClearRank(RoomData[] roomData)
        {
            int rank = 1;
            int roopCnt = 0;
            while (roopCnt < roomData.Length)
            {
                roopCnt = 0;
                for (int i = roomData.Length - 1; i >= 0; i--, roopCnt++)
                {
                    if (roomData[i].UserState.areaGoalRank == rank)
                    {
                        rank++;
                        break;
                    }
                }
            }
            return rank;
        }

        /// <summary>
        /// エリアをクリアした人数を取得
        /// </summary>
        /// <param name="roomData"></param>
        /// <returns></returns>
        int GetAreaClearedUsersCount(RoomData[] roomData)
        {
            int count = 0;
            foreach (var data in roomData) 
            {
                if(data.UserState.isAreaCleared) count++;
            }
            return count;
        }

        /// <summary>
        /// 次のエリアに移動する準備が完了した処理
        /// </summary>
        /// <returns></returns>
        public async Task ReadyNextAreaAsynk()
        {
            var roomStorage = room.GetInMemoryStorage<RoomData>();

            // [排他制御] 次のエリアに移動する準備チェックが複数同時に処理すると、データの整合性に異常がでるため
            lock (roomStorage)
            {
                // 送信したユーザーのデータを更新
                var data = roomStorage.Get(this.ConnectionId);
                data.UserState.isReadyNextArea = true;
                Console.WriteLine(data.JoinedUser.UserData.Name + "の準備");

                // 送信したユーザーがエリアをクリアできなかった場合
                RoomData[] roomDataList = roomStorage.AllValues.ToArray<RoomData>();
                if (!data.UserState.isAreaCleared)
                {
                    data.UserState.areaGoalRank = GetAreaClearedUsersCount(roomDataList) + 1;
                    data.JoinedUser.score += baseAddScore / data.UserState.areaGoalRank;
                }

                // 全員次のエリアに移動する準備が完了したかチェック
                int readyCnt = 0;
                bool isRedyAllUsers = false;
                foreach (var roomData in roomDataList)
                {
                    if (roomData.UserState.isReadyNextArea) readyCnt++;
                }
                if (readyCnt == roomDataList.Length) isRedyAllUsers = true;

                if (isRedyAllUsers)
                {
                    foreach (var roomData in roomDataList)
                    {
                        roomData.JoinedUser.IsStartMasterCountDown = false;
                    }

                    // 次のエリアに移動させる準備
                    Console.WriteLine("再開通知");
                    const float baseWaitSec = 0.8f;
                    foreach (var roomData in roomDataList)
                    {
                        float waitSec = (roomData.UserState.areaGoalRank + 1) * baseWaitSec;

                        // ゲーム再開通知を個別に配る
                        this.BroadcastTo(room, roomData.JoinedUser.ConnectionId).OnReadyNextAreaAllUsers(waitSec);

                        // 情報をリセット
                        roomData.UserState.isAreaCleared = false;
                        roomData.UserState.areaGoalRank = 0;
                        roomData.UserState.isReadyNextArea = false;
                    }
                }
            }
        }

        /// <summary>
        /// カウントダウン処理
        /// (マスタークライアントが繰り返し呼び出し)
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public async Task CountDownAsynk(int currentTime)
        {
            this.Broadcast(room).OnCountDown(currentTime);
        }

        /// <summary>
        /// 最終結果発表シーンに遷移した通知
        /// </summary>
        /// <returns></returns>
        public async Task TransitionFinalResultSceneAsynk()
        {
            var roomStorage = room.GetInMemoryStorage<RoomData>();

            // [排他制御] 遷移したかどうかチェックが複数同時に処理すると、データの整合性に異常がでるため
            lock (roomStorage)
            {
                // 送信したユーザーのデータを更新
                var data = roomStorage.Get(this.ConnectionId);
                data.UserState.isTransitionFinalResultScene = true;
                Console.WriteLine(data.JoinedUser.UserData.Name + "が最終結果発表シーンに移動");

                // 全員が遷移したかどうかチェック
                RoomData[] roomDataList = roomStorage.AllValues.ToArray<RoomData>();
                int transitionCnt = 0;
                foreach (var roomData in roomDataList)
                {
                    if (roomData.UserState.isTransitionFinalResultScene) transitionCnt++;
                }

                if (transitionCnt == roomDataList.Length)
                {
                    // 全員が遷移できた通知
                    this.Broadcast(room).OnTransitionFinalResultSceneAllUsers(GetResultData(roomDataList));
                }
            }
        }

        ResultData[] GetResultData(RoomData[] roomData)
        {
            ResultData[] resultData = new ResultData[roomData.Length];
            for (int i = 0; i < resultData.Length; i++)
            {
                resultData[i] = new ResultData() { 
                    connectionId = roomData[i].JoinedUser.ConnectionId,
                    joinOrder = roomData[i].JoinedUser.JoinOrder,
                    rank = 0,
                    score = roomData[i].JoinedUser.score
                };
            }

            // スコアを基準に降順に並び替える
            var sortList = resultData.OrderByDescending(d => d.score);
            int lastScore = 0;
            int lastRank = 1;
            foreach (var sortData in sortList) 
            {
                for (int i = 0; i < resultData.Length; i++) 
                {
                    if (resultData[i].connectionId == sortData.connectionId) 
                    {
                        if(lastScore != resultData[i].score)
                        {
                            lastRank = lastScore == 0 ? 1 : lastRank + 1;
                        }
                        lastScore = resultData[i].score;
                        resultData[i].rank = lastRank;
                        break;
                    }
                }

                Console.WriteLine(sortData.connectionId + ":" + lastRank + "位,score:" + sortData.score);
            }

            return resultData;
        }
    }
}

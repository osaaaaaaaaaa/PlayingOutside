using MagicOnion.Server.Hubs;
using Server.Model.Context;
using Shared.Interfaces.Model.Entity;
using Shared.Interfaces.StreamingHubs;
using System.Runtime.Intrinsics.X86;

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

        /// <summary>
        /// ユーザーの切断処理
        /// </summary>
        /// <returns></returns>
        protected override ValueTask OnDisconnected()
        {
            Console.WriteLine("切断検知");

            // 入室した状態で切断した場合
            if (this.room.GetInMemoryStorage<RoomData>().Get(this.ConnectionId) != null)
            {
                // ルームデータを削除
                this.room.GetInMemoryStorage<RoomData>().Remove(this.ConnectionId);

                // 退室したことを全メンバーに通知
                this.Broadcast(room).OnLeave(this.ConnectionId);
            }

            // ルーム内のメンバーから削除
            room.RemoveAsync(this.Context);
            return CompletedTask;
        }

        /// <summary>
        /// 入室処理
        /// </summary>
        /// <param name="roomName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<JoinedUser[]> JoinAsynk(string roomName, int userId)
        {
            // 指定したルームに参加、ルームを保持
            this.room = await this.Group.AddAsync(roomName);

            // DBからユーザー情報取得
            GameDbContext context = new GameDbContext();
            var user = context.Users.Where(user => user.Id == userId).FirstOrDefault();

            // グループストレージにユーザーデータを格納
            var roomStorage = this.room.GetInMemoryStorage<RoomData>(); // ストレージには一種類の型しか使えないため、他の情報を入れたい場合は、RoomDataクラスに追加
            int joinOrder = GetJoinOrder(roomStorage.AllValues.ToArray<RoomData>());
            var joinedUser = new JoinedUser() { ConnectionId = this.ConnectionId, UserData = user, JoinOrder = joinOrder　};
            var roomData = new RoomData() { JoinedUser = joinedUser ,PlayerState = new PlayerState(), UserState = new UserState()};
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

            Console.WriteLine(roomData.JoinedUser.ConnectionId + "：" + roomData.JoinedUser.UserData.Name + "が入室");

            return joinedUserList;
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
            Console.WriteLine(roomStorage.Get(this.ConnectionId).JoinedUser.UserData.Name + "が退室しました");

            // ルーム参加者にユーザーの退室通知を送信
            this.Broadcast(room).OnLeave(this.ConnectionId);

            // 自分のデータを グループデータから削除する
            this.room.GetInMemoryStorage<RoomData>().Remove(this.ConnectionId);
            // ルーム内のメンバーから自分を削除
            await room.RemoveAsync(this.Context);
        }

        /// <summary>
        /// プレイヤー情報更新
        /// </summary>
        /// <returns></returns>
        public async Task UpdatePlayerStateAsynk(PlayerState state)
        {
            var roomStorage = room.GetInMemoryStorage<RoomData>();

            // ストレージ内のプレイヤー情報を更新する
            var data =  roomStorage.Get(this.ConnectionId);
            data.PlayerState = state;

            // ルーム参加者にプレイヤー情報更新通知を送信
            this.BroadcastExceptSelf(room).OnUpdatePlayerState(this.ConnectionId, state);
        }

        /// <summary>
        /// 準備完了したかどうか
        /// </summary>
        /// <returns></returns>
        public async Task OnReadyAsynk(bool isReady)
        {
            // 送信したユーザーのデータを更新
            var roomStorage = room.GetInMemoryStorage<RoomData>();
            var data = roomStorage.Get(this.ConnectionId);
            data.UserState.isReady = isReady;
            Console.WriteLine(data.JoinedUser.UserData.Name + "の準備:" + isReady);

            // 全員が準備完了したかどうかチェック
            bool isAllUsersReady = false;

            int readyCnt = 0;
            RoomData[] roomDataList = roomStorage.AllValues.ToArray<RoomData>();
            foreach (var roomData in roomDataList)
            {
                if (roomData.UserState.isReady) readyCnt++;
            }

            // 最低人数以上かつ全員が準備完了している場合
            if (roomDataList.Length >= minRequiredUsers && readyCnt == roomDataList.Length) isAllUsersReady = true;

            // 準備完了通知
            this.Broadcast(room).OnReady(readyCnt, isAllUsersReady);
        }
        

        /// <summary>
        /// ゲーム開始前のカウントダウンが終了
        /// </summary>
        /// <returns></returns>
        public async Task OnCountdownOverAsynk()
        {
            // 送信したユーザーのデータを更新
            var roomStorage = room.GetInMemoryStorage<RoomData>();
            var data = roomStorage.Get(this.ConnectionId);
            data.UserState.isCountdownOver = true;
            Console.WriteLine(data.JoinedUser.UserData.Name + "：" + "カウントダウンが終了");

            // 全員がカウントダウン終了したかどうかチェック
            int readyCnt = 0;
            RoomData[] roomDataList = roomStorage.AllValues.ToArray<RoomData>();
            foreach (var roomData in roomDataList)
            {
                if (roomData.UserState.isCountdownOver) readyCnt++;
            }

            // ゲーム開始通知を配る
            if (readyCnt == roomDataList.Length) this.Broadcast(room).OnCountdownOver();
        }
    }
}

using MagicOnion.Server.Hubs;
using Server.Model.Context;
using Shared.Interfaces.Model.Entity;
using Shared.Interfaces.StreamingHubs;

namespace Server.StreamingHubs
{
    public class RoomHub : StreamingHubBase<IRoomHub, IRoomHubReceiver>, IRoomHub
    {
        private IGroup room;    // どのルームに入っているか
        public async Task<JoinedUser[]> JoinAsynk(string roomName, int userId)
        {
            // 指定したルームに参加、ルームを保持
            this.room = await this.Group.AddAsync(roomName);

            // DBからユーザー情報取得
            GameDbContext context = new GameDbContext();
            var user = context.Users.Where(user => user.Id == userId).FirstOrDefault();

            // グループストレージにユーザーデータを格納
            var roomStorage = this.room.GetInMemoryStorage<RoomData>(); // ストレージには一種類の型しか使えないため、他の情報を入れたい場合は、RoomDataクラスに追加
            var joinedUser = new JoinedUser() { ConnectionId = this.ConnectionId , UserData = user};
            var roomData = new RoomData() { JoinedUser = joinedUser };
            roomStorage.Set(this.ConnectionId,roomData);    // 自動で割り当てされるユーザーごとの接続IDに紐づけて保存したいデータを格納する

            // 自分以外のルーム参加者全員に、ユーザーの入室通知を送信(Broodcast:配布する,Except:自分以外)
            // ※Broadcast(room) で自身も含めて関数を実行できる
            this.BroadcastExceptSelf(room).OnJoin(joinedUser);

            // ルームデータ(グループストレージ内のデータ)情報取得
            RoomData[] roomDataList = roomStorage.AllValues.ToArray<RoomData>();

            // 参加中のユーザー情報を返す
            JoinedUser[] joinedUserList = new JoinedUser[roomDataList.Length];
            for(int i = 0; i < joinedUserList.Length; i++)
            {
                joinedUserList[i] = roomDataList[i].JoinedUser;
            }
            return joinedUserList;
        }
    }
}

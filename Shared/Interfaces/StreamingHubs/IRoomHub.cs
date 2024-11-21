using MagicOnion;
using Shared.Interfaces.Model.Entity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces.StreamingHubs
{
    public interface IRoomHub : IStreamingHub<IRoomHub, IRoomHubReceiver>
    {
        // クライアント側からサーバー側を呼び出す関数を定義する

        // ユーザー入室
        Task<JoinedUser[]> JoinAsynk(string roomName,int userId);

        // ユーザー退室
        Task LeaveAsynk();

        // プレイヤー情報更新
        Task UpdatePlayerStateAsynk(PlayerState state);
    }
}

using Shared.Interfaces.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Interfaces.StreamingHubs
{
    internal interface IRoomHubReceiver
    {
        // [ここにサーバー側からクライアントを呼び出す関数を定義]

        // ユーザーの入室通知
        void OnJoin(JoinedUser user);
    }
}

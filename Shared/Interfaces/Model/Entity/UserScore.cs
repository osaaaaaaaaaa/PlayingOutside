//*********************************************************
// ルームに参加中のユーザースコア
// Author:Rui Enomoto
//*********************************************************
using MessagePack;
using Server.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Interfaces.Model.Entity
{
    [MessagePackObject]
    public class UserScore
    {
        [Key(0)]
        public Guid ConnectionId { get; set; }
        [Key(1)]
        public int LatestScore { get; set; }  // 更新後の所持ポイント
    }
}

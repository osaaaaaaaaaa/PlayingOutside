using MessagePack;
using Server.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Interfaces.Model.Entity
{
    [MessagePackObject]
    public class JoinedUser
    {
        [Key(0)]
        public Guid ConnectionId { get; set; }
        [Key(1)]
        public User UserData { get; set; }
        [Key(2)]
        public int JoinOrder { get; set; }  // 入室順
        [Key(3)]
        public bool IsMasterClient { get; set; }    // マスタークライアントかどうか
        [Key(4)]
        public bool IsStartMasterCountDown { get; set; }    // マスタークライアントがカウントダウンを開始しているかどうか
        [Key(5)]
        public bool IsGameRunning { get; set; }    // ゲーム中かどうか
        [Key(6)]
        public bool IsMatching { get; set; }    // 自動マッチングかが完了しているかどうか
    }
}

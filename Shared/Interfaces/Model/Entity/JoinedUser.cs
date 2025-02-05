//*********************************************************
// ルームに参加中のユーザー情報
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
        [Key(7)]
        public int score { get; set; }  // 所持ポイント
        [Key(8)]
        public int rating { get; set; }
        [Key(9)]
        public bool IsFinishMasterCountDown { get; set; }   // マスタークライアントのカウントダウンが終了しているかどうか
        [Key(10)]
        public EnumManager.SELECT_RELAY_AREA_ID selectMidAreaId { get; set; } = EnumManager.SELECT_RELAY_AREA_ID.Course_Random; // [競技：カントリーリレー]事前に選択した中間エリアのID
        [Key(11)]
        public EnumManager.SELECT_FINALGAME_AREA_ID selectFinalStageId { get; set; } = EnumManager.SELECT_FINALGAME_AREA_ID.Stage_Random; // [競技：乱闘]事前に選択したステージのID
    }
}

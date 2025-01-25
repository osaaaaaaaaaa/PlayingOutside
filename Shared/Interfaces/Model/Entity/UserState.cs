using MessagePack;
using Server.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Shared.Interfaces.Model.Entity
{
    [MessagePackObject]
    public class UserState
    {
        [Key(0)]
        public bool isReadyRoom { get; set; }   // ルームシーンで準備完了をしたかどうか
        [Key(1)]
        public bool isCountdownOver { get; set; }   // ゲーム開始前のカウントダウンが終了したかどうか
        [Key(2)]
        public bool isAreaCleared { get; set; } // [競技：カントリーリレー]現在のエリアをクリアしたかどうか
        [Key(3)]
        public int areaGoalRank { get; set; }  // [競技：カントリーリレー]現在のエリアをクリアしたときの順位
        [Key(4)]
        public bool isReadyNextArea { get; set; }   // [競技：カントリーリレー]次のエリアに進む準備完了したかどうか
        [Key(5)]
        public bool isTransitionFinalResultScene { get; set; }  // 最終結果発表シーンに遷移したかどうか
        [Key(6)]
        public int FinishGameCnt { get; set; }    // 現在のゲーム数
        [Key(7)]
        public bool isFinishGame { get; set; }    // 現在のゲームが終了したかどうか
        [Key(8)]
        public List<string> usedItemNameList { get; set; } = new List<string>();    // アイテムの使用履歴
        [Key(9)]
        public bool isDestroyPlantsRequest { get; set; } = false;    // [競技：カントリーリレー]植物のギミックを破棄するリクエスト(送信or受信)をしたかどうか
        [Key(10)]
        public List<string> triggeringPlantGimmickList { get; set; } = new List<string>();    // [競技：カントリーリレー]植物のギミックを発動した履歴
        [Key(11)]
        public EnumManager.RELAY_AREA_ID currentAreaId { get; set; } = EnumManager.FirstAreaId;    // [競技：カントリーリレー]現在のエリアのID
    }
}

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
        public bool isReadyRoom { get; set; }
        [Key(1)]
        public bool isCountdownOver { get; set; }
        [Key(2)]
        public bool isAreaCleared { get; set; }
        [Key(3)]
        public int areaGoalRank {  get; set; }
        [Key(4)]
        public int score { get; set; }
        [Key(5)]
        public bool isReadyNextArea { get; set; }
        [Key(6)]
        public bool isTransitionFinalResultScene { get; set; }
    }
}

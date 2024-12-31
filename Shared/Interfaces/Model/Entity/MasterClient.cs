using MessagePack;
using Server.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Shared.Interfaces.Model.Entity
{
    [MessagePackObject]
    public class MasterClient
    {
        [Key(0)]
        public PlayerState playerState { get; set; }    // ユーザーの情報
        [Key(2)]
        public List<MovingObjectState> objectStates { get; set; } = new List<MovingObjectState>(); // 消えない動くオブジェクトの情報
    }
}

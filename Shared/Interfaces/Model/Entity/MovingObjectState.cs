//*********************************************************
// ゲーム中に動き続けるギミックの同期情報
// Author:Rui Enomoto
//*********************************************************
using MessagePack;
using Server.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Shared.Interfaces.Model.Entity
{
    [MessagePackObject]
    public class MovingObjectState
    {
        [Key(0)]
        public string name { get; set; }
        [Key(1)]
        public Vector3 position { get; set; }
        [Key(2)]
        public Vector3 angle { get; set; }
        [Key(3)]
        public float elapsedTimeTween { get; set; } // Tweenの現在の再生時間
        [Key(4)]
        public bool isActiveSelf { get; set; }
    }
}

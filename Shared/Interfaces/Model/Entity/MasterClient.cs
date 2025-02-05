//*********************************************************
// ゲーム中のマスタークライアントの情報(各ルームにつき1人)
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
    public class MasterClient
    {
        [Key(0)]
        public PlayerState playerState { get; set; }    // ユーザーの情報
        [Key(1)]
        public List<MovingObjectState> objectStates { get; set; } = new List<MovingObjectState>(); // Rootに沿って動くオブジェクトの更新情報
        [Key(2)]
        public List<GooseState> gooseStates { get; set; } = new List<GooseState>(); // ガチョウの更新情報
    }
}

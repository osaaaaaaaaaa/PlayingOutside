//*********************************************************
// 最終結果発表シーンで使用するユーザーのリザルト情報
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
    public class ResultData
    {
        [Key(0)]
        public Guid connectionId { get; set; }
        [Key(1)]
        public int joinOrder { get; set; }
        [Key(2)]
        public int rank { get; set; }
        [Key(3)]
        public int score { get; set; }
    }
}

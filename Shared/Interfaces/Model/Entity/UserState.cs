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
        public bool isReady { get; set; }
        [Key(1)]
        public bool isCountdownOver { get; set; }
    }
}

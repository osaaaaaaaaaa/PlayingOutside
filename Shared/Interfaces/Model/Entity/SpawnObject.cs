using MessagePack;
using Server.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Shared.Interfaces.Model.Entity
{
    [MessagePackObject]
    public class SpawnObject
    {
        [Key(0)]
        public string name { get; set; }
        [Key(1)]
        public Vector3 position { get; set; }
        [Key(2)]
        public Vector3 angle { get; set; }
        [Key(3)]
        public Vector3 forse { get; set; }
        [Key(4)]
        public EnumManager.SPAWN_OBJECT_ID objectId { get; set; }
    }
}

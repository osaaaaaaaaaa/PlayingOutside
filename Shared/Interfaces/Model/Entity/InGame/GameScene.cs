using MessagePack;
using Server.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Interfaces.Model.Entity
{
    [MessagePackObject]
    public class GameScene
    {
        public enum SCENE_ID
        {
            RelayGame = 0,
            FinalGame,
        }

        [Key(0)]
        public SCENE_ID GameSceneId { get; set; }
    }
}

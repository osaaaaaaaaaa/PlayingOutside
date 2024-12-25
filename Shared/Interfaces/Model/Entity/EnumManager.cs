using MessagePack;
using Server.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Interfaces.Model.Entity
{
    [MessagePackObject]
    public class EnumManager
    {
        public enum SCENE_ID
        {
            RelayGame = 0,
            FinalGame,
        }

        public enum ITEM_ID
        {
            Coin = 0,
            Pepper,
        }
    }
}

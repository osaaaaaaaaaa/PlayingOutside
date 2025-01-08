using MessagePack;
using Server.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Interfaces.Model.Entity
{
    public class EnumManager
    {
        public enum Character_ID
        {
            OriginalHiyoko = 1,
            ChickenHiyoko,
            BrackHiyoko,
            StarHiyoko,
            GoldHiyoko,
        }

        public enum SCENE_ID
        {
            RelayGame = 0,
            FinalGame,
        }

        public enum ITEM_ID
        {
            None = 0,
            ItemBox,
            Coin,
            Pepper,
        }

        public enum ITEM_EFFECT_TIME
        {
            Pepper = 7,
        }

        public enum SPAWN_OBJECT_ID
        {
            Hay = 0,
        }
    }
}

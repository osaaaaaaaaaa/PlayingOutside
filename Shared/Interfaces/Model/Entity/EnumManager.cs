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

        public enum RELAY_AREA_ID
        {
            Area1_Mud,
            Area2_Hay,
            Area3_Cow,
            Area4_Plant,
            Area5_Goose,
            Area6_Chicken
        }
        public static RELAY_AREA_ID FirstAreaId = RELAY_AREA_ID.Area1_Mud;
        public static RELAY_AREA_ID MiddleAreaMinId = RELAY_AREA_ID.Area2_Hay;
        public static RELAY_AREA_ID MiddleAreaMaxId = RELAY_AREA_ID.Area5_Goose;
        public static RELAY_AREA_ID LastAreaId { get; private set; } = RELAY_AREA_ID.Area6_Chicken;


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

        public enum ANIMAL_GIMMICK_ID
        {
            Bull = 0,
            Chicken,
        }
    }
}

using MessagePack;
using Server.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Interfaces.Model.Entity
{
    [MessagePackObject]
    public class Item
    {
        public enum ITEM_ID
        {
            Coin = 0,
        }

        [Key(0)]
        public ITEM_ID ItemId { get; set; }
    }
}

using MessagePack;
using System;
using System.Security;

namespace Server.Model.Entity
{
    [MessagePackObject]
    public class RatingRanking
    {
        [Key(0)]
        public string UserName { get; set; }
        [Key(1)]
        public int CharacterId { get; set; }
        [Key(2)]
        public int Rating { get; set; }
    }
}

using MessagePack;
using System;

namespace Server.Model.Entity
{
    public class Rating
    {
        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public int user_id { get; set; }
        [Key(2)]
        public int rating { get; set; }
        [Key(3)]
        public DateTime Created_at { get; set; }
        [Key(4)]
        public DateTime Updated_at { get; set; }
    }
}

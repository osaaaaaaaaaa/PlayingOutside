using MessagePack;
using System;

namespace Server.Model.Entity
{
    [MessagePackObject]
    public class Follow
    {
        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public int Following_id { get; set; }   // フォローしている人のID
        [Key(2)]
        public int Followee_id { get; set; }    // フォローされている人のID
        [Key(3)]
        public DateTime Created_at { get; set; }
        [Key(4)]
        public DateTime Updated_at { get; set; }
    }
}

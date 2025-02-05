//*********************************************************
// NGワード情報
// Author:Rui Enomoto
//*********************************************************
using MessagePack;
using System;

namespace Server.Model.Entity
{
    [MessagePackObject]
    public class NGWord
    {
        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public string Word { get; set; }
        [Key(2)]
        public DateTime Created_at { get; set; }
        [Key(3)]
        public DateTime Updated_at { get; set; }
    }
}

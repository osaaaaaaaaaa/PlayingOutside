//*********************************************************
// フォローしているユーザー情報
// Author:Rui Enomoto
//*********************************************************
using MessagePack;
using System;
using System.Security;

namespace Server.Model.Entity
{
    [MessagePackObject]
    public class FollowingUser
    {
        [Key(0)]
        public int UserId { get; set; }
        [Key(1)]
        public string UserName { get; set; }
        [Key(2)]
        public int CharacterId { get; set; }
        [Key(3)]
        public int Rating { get; set; }
        [Key(4)]
        public bool IsMutualFollow {  get; set; }   // 相互フォローかどうか
    }
}

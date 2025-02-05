//*********************************************************
// ユーザー情報
// Author:Rui Enomoto
//*********************************************************
using MessagePack;
using System;
using System.Collections.Generic;

namespace Server.Model.Entity
{
    [MessagePackObject]
    /// <summary>
    /// Entiitiy配下にテーブルクラス作成
    /// </summary>
    public class User
    {
        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public string Name { get; set; }
        [Key(2)]
        public string Token { get; set; }
        [Key(3)]
        public int Character_Id { get; set; }
        [Key(4)]
        public DateTime Created_at { get; set; }
        [Key(5)]
        public DateTime Updated_at { get; set; }
    }
}

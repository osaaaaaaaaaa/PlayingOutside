using MessagePack;
using Server.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Interfaces.Model.Entity
{
    [MessagePackObject]
    public class JoinedUser
    {
        [Key(0)]
        public Guid ConnectionId { get; set; }
        [Key(1)]
        public User UserData { get; set; }
        [Key(2)]
        public int JoinOrder { get; set; }
    }
}

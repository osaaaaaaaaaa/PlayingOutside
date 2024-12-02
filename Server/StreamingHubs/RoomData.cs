using Shared.Interfaces.Model.Entity;

namespace Server.StreamingHubs
{
    public class RoomData
    {
        public JoinedUser JoinedUser {  get; set; }
        public PlayerState PlayerState { get; set; }
        public UserState UserState { get; set; }
    }
}

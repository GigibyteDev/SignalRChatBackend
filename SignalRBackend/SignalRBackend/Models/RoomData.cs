namespace SignalRBackend.Models
{
    public class RoomData
    {
        public List<Message> Messages { get; private set; }
        public List<UserConnection> Users { get; private set; }

        public int TotalUsers
        {
            get => Users.Count;
        }

        public RoomData(UserConnection initialUser)
        {
            Messages = new List<Message>();
            Users = new List<UserConnection>() { initialUser };
        }
    }
}

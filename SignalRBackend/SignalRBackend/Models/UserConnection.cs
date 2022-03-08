using SignalRBackend.Helpers;

namespace SignalRBackend.Models
{
    public class UserConnection
    {
        public int UserId { get; private set; } = MessageIDProvider.NewUserId;
        public string Username { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;

        public static UserConnection BotUser { get; } = new UserConnection(0, "ChatBot");

        public UserConnection(string userName, string room)
        {
            Username = userName;
            Room = room;
        }

        private UserConnection(int userId, string userName)
        {
            UserId = userId;
            Username = userName;
        }
    }
}

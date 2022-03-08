using SignalRBackend.Helpers;

namespace SignalRBackend.Models
{
    public class Message
    {
        public int Id { get; private set; } = MessageIDProvider.NewMessageId;
        public UserConnection User { get; set; }
        public string MessageBody { get; set; }

        public Message(UserConnection user, string message)
        {
            User = user;
            MessageBody = message;
        }
    }
}

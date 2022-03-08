namespace SignalRBackend.Helpers
{
    public class MessageIDProvider
    {
        private static int _messageId = 0;
        private static int _userId = 0;
        public static int NewMessageId
        {
            get => ++_messageId;
        }

        public static int NewUserId
        {
            get => ++_userId;
        }
    }
}

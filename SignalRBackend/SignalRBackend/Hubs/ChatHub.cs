using Microsoft.AspNetCore.SignalR;
using SignalRBackend.Models;

namespace SignalRBackend.Hubs
{
    public class ChatHub : Hub
    {
        // Memcache
        private readonly IDictionary<string, UserConnection> _userConnections;
        private readonly IDictionary<string, RoomData> _rooms;
        public ChatHub(IDictionary<string, UserConnection> connections, IDictionary<string, RoomData> rooms)
        {
            _userConnections = connections;
            _rooms = rooms;
        }

        public async Task JoinRoom(string userName, string roomName)
        {
            UserConnection userConnection;
            if (_userConnections.TryGetValue(Context.ConnectionId, out UserConnection? previousUserConnection))
            {
                userConnection = previousUserConnection;
                userConnection.Username = userName;
                userConnection.Room = roomName;
                _userConnections[Context.ConnectionId] = userConnection;
            }
            else
            {
                userConnection = new UserConnection(userName, roomName);
                _userConnections.Add(Context.ConnectionId, userConnection);
            }

            if (_rooms.TryGetValue(userConnection.Room, out RoomData? roomData))
            {
                roomData.Users.Add(userConnection);
            }
            else
            {
                _rooms.Add(userConnection.Room, new RoomData(userConnection));
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);

            await SendInstantiateUserAsync(userConnection.Room, userConnection.UserId);

            await SendReceiveMessageFromBotAsync(userConnection.Room, $"{userConnection.Username} has Joined");
        }

        public async Task LeaveRoom()
        {
            if (_userConnections.TryGetValue(Context.ConnectionId, out UserConnection? userConnection))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userConnection.Room);

                await SendReceiveMessageFromBotAsync(userConnection.Room, $"{userConnection.Username} has Left");

                if (_rooms.TryGetValue(userConnection.Room, out RoomData? roomData))
                {
                    roomData.Users.Remove(userConnection);

                    if (roomData.TotalUsers == 0)
                    {
                        _rooms.Remove(userConnection.Room);
                    }
                }
                await SendUserLoggedOutAsync();
            }
        }

        public async Task SendMessage(string message)
        {
            if (_userConnections.TryGetValue(Context.ConnectionId, out UserConnection? userConnection))
            {
                Message newMessage = new Message(userConnection, message);
                await SendReceiveMessageAsync(newMessage);
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Task.Run(() => LeaveRoom());

            return base.OnDisconnectedAsync(exception);
        }

        private async Task SendInstantiateUserAsync(string room, int userId)
        {
            if (_rooms.TryGetValue(room, out RoomData? roomData))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("InstantiateUser", userId, roomData.Messages);
            }
        }

        private async Task SendUserLoggedOutAsync()
        {
            await Clients.Client(Context.ConnectionId).SendAsync("UserLoggedOut");
        }

        private async Task SendReceiveMessageAsync(Message message)
        {
            await Clients.Group(message.User.Room).SendAsync("ReceiveMessage", message);
            AddMessageToLocalRoomData(message.User.Room, message);
        }

        private async Task SendReceiveMessageFromBotAsync(string room, string message)
        {
            Message messageToAdd = new Message(UserConnection.BotUser, message);
            await Clients.Group(room).SendAsync("ReceiveMessage", messageToAdd);
            AddMessageToLocalRoomData(room, messageToAdd);
        }

        private void AddMessageToLocalRoomData(string room, Message message)
        {
            if (_rooms.TryGetValue(room, out RoomData? roomData))
            {
                roomData.Messages.Add(message);
            }
        }
    }
}

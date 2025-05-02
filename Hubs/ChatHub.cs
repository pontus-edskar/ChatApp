using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ChatApp.DataService;
using ChatApp.Models;


namespace ChatApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly SharedDb _sharedDb;

        public ChatHub(SharedDb sharedDb)
        {
            _sharedDb = sharedDb;
        }

        public async Task JoinChatRoom(string userName, string chatRoom, string role)
        {
            var userConnection = new UserConnection
            {
                UserName = userName,
                ChatRoom = chatRoom,
                Role = role
            };

            _sharedDb.Connection[Context.ConnectionId] = userConnection;

            string generalGroup = $"{chatRoom}_general";
            string announcementGroup = $"{chatRoom}_announcement";

            await Groups.AddToGroupAsync(Context.ConnectionId, generalGroup);
            await Groups.AddToGroupAsync(Context.ConnectionId, announcementGroup);

            await Clients.Group(generalGroup).SendAsync("ReceiveMessage", "admin", $"{userName} ({role}) joined {chatRoom}", generalGroup);


        }

        public async Task SendMessage(string chatRoom, string userName, string message)
        {
            if(_sharedDb.Connection.TryGetValue(Context.ConnectionId, out var user))
            {
                bool isAnnouncement = chatRoom.EndsWith("_announcement");

                if(isAnnouncement && user.Role != "teacher")
                {
                    return;
                }
            }

            await Clients.Group(chatRoom).SendAsync("ReceiveMessage", userName, message, chatRoom);
        }
    }
}
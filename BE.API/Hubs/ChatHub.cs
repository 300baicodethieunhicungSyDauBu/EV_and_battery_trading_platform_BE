using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BE.API.Hubs
{
    public class ChatHub : Hub
    {
        // Khi user join chat (theo ChatId)
        public async Task JoinChat(string chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
            await Clients.Group(chatId).SendAsync("UserJoined", $"{Context.ConnectionId} joined chat {chatId}");
        }

        // Khi user rời chat
        public async Task LeaveChat(string chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
            await Clients.Group(chatId).SendAsync("UserLeft", $"{Context.ConnectionId} left chat {chatId}");
        }
    }
}
using Microsoft.AspNetCore.SignalR;

namespace Core.Hubs
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                await Clients.All.SendAsync("UserStatusChanged", int.Parse(userId), true);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                await Clients.All.SendAsync("UserStatusChanged", int.Parse(userId), false);
            }
            await base.OnDisconnectedAsync(ex);
        }

        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
        }

        public async Task UserTyping(int conversationId, bool isTyping, string userTyping)
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                await Clients.OthersInGroup(conversationId.ToString()).SendAsync("UserTyping", conversationId, isTyping, userTyping);
            }
        }
    }
}

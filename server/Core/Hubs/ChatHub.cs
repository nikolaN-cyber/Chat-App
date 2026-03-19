using Microsoft.AspNetCore.SignalR;

namespace Core.Hubs
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name;
            await Clients.All.SendAsync("UserStatusChanged", username, true);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var username = Context.User?.Identity?.Name;
            await Clients.All.SendAsync("UserStatusChanged", username, false);
            await base.OnDisconnectedAsync(ex);
        }

        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
        }
    }
}

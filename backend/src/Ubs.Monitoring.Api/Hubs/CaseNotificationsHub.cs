using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Ubs.Monitoring.Api.Hubs;

[Authorize]
public sealed class CaseNotificationsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // All analysts receive all case alerts
        await Groups.AddToGroupAsync(Context.ConnectionId, "analysts");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "analysts");
        await base.OnDisconnectedAsync(exception);
    }
}

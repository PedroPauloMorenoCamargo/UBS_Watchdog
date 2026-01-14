using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Ubs.Monitoring.Api.Hubs;

[Authorize]
public sealed class CaseNotificationsHub : Hub
{
    /// <summary>
    /// Handles logic executed when a client establishes a connection to the SignalR hub.
    /// </summary>
    /// <remarks>
    /// All authenticated analysts are automatically added to the <c>analysts</c> group to receive case notification events.
    /// </remarks>
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "analysts");
        await base.OnConnectedAsync();
    }
    /// <summary>
    /// Handles logic executed when a client disconnects from the SignalR hub.
    /// </summary>
    /// <param name="exception">
    /// The exception that caused the disconnection, if any.
    /// </param>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "analysts");
        await base.OnDisconnectedAsync(exception);
    }
}

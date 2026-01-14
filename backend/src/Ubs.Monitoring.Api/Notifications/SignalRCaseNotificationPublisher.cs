using Microsoft.AspNetCore.SignalR;
using Ubs.Monitoring.Api.Hubs;
using Ubs.Monitoring.Application.Cases.Notifications;

namespace Ubs.Monitoring.Api.Notifications;

public sealed class SignalRCaseNotificationPublisher : ICaseNotificationPublisher
{
    private readonly IHubContext<CaseNotificationsHub> _hub;

    public SignalRCaseNotificationPublisher(IHubContext<CaseNotificationsHub> hub)
    {
        _hub = hub;
    }
    /// <summary>
    /// Publishes a notification indicating that a case has been opened.
    /// </summary>
    /// <param name="notification">
    /// The notification payload containing details about the opened case.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// A task that completes when the notification has been dispatched to all connected analyst clients.
    /// </returns>
    public Task PublishCaseOpenedAsync( CaseOpenedNotification notification, CancellationToken ct)
    {
        return _hub.Clients.Group("analysts").SendAsync("caseOpened", notification, ct);
    }
}

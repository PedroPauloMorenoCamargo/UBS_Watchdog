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

    public Task PublishCaseOpenedAsync( CaseOpenedNotification notification, CancellationToken ct)
    {
        return _hub.Clients.Group("analysts").SendAsync("caseOpened", notification, ct);
    }
}

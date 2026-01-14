namespace Ubs.Monitoring.Application.Cases.Notifications;

public interface ICaseNotificationPublisher
{
    Task PublishCaseOpenedAsync(CaseOpenedNotification notification, CancellationToken ct);
}

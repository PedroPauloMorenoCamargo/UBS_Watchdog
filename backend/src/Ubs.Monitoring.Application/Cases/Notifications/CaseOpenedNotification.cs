namespace Ubs.Monitoring.Application.Cases.Notifications;

public sealed record CaseOpenedNotification(
    Guid CaseId,
    Guid ClientId,
    Guid? AccountId,
    int Severity,
    DateTimeOffset OpenedAtUtc
);
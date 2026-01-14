namespace Ubs.Monitoring.Application.Cases.Notifications;


/// <summary>
/// Represents a notification emitted when a new case is opened.
/// </summary>
/// <param name="CaseId">
/// The unique identifier of the opened case.
/// </param>
/// <param name="ClientId">
/// The unique identifier of the client associated with the case.
/// </param>
/// <param name="AccountId">
/// The identifier of the related account, if applicable.
/// </param>
/// <param name="Severity">
/// The severity level associated with the case at the time it was opened.
/// </param>
/// <param name="OpenedAtUtc">
/// The UTC timestamp indicating when the case was opened.
/// </param>
public sealed record CaseOpenedNotification(
    Guid CaseId,
    Guid ClientId,
    Guid? AccountId,
    int Severity,
    DateTimeOffset OpenedAtUtc
);
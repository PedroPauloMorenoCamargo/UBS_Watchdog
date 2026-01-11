namespace Ubs.Monitoring.Application.Transactions.Scheduling;

public sealed class ScheduleTransactionRequest
{
    public Guid AccountId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public DateTimeOffset ScheduledForUtc { get; init; }
}

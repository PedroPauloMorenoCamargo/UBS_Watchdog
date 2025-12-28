using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class Case
{
    public Guid Id { get; set; }

    public Guid TransactionId { get; set; }
    public Transaction Transaction { get; set; } = null!;

    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public CaseStatus Status { get; set; }
    public CaseDecision? Decision { get; set; }

    public Guid? AnalystId { get; set; }
    public Analyst? Analyst { get; set; }

    public Severity Severity { get; set; }

    public DateTimeOffset OpenedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public DateTimeOffset? ResolvedAtUtc { get; set; }

    public ICollection<CaseFinding> Findings { get; set; } = new List<CaseFinding>();
}

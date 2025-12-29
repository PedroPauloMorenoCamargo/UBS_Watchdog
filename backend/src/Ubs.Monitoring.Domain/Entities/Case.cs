using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class Case
{
    public Guid Id { get; private set; }

    public Guid TransactionId { get; private set; }
    public Transaction Transaction { get; set; } = null!;

    public Guid ClientId { get; private set; }
    public Client Client { get; set; } = null!; 

    public Guid AccountId { get; private set; }
    public Account Account { get; set; } = null!;

    public CaseStatus Status { get; private set; } 
    public CaseDecision? Decision { get; private set; } 

    public Guid? AnalystId { get; private set; }
    public Analyst? Analyst { get; set; } 

    public Severity Severity { get; private set; } 

    public DateTimeOffset OpenedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? ResolvedAtUtc { get; private set; } 

    public ICollection<CaseFinding> Findings { get; set; } = new List<CaseFinding>();
}

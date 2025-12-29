using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class Case
{
    private Case()
    {
        _findings = new List<CaseFinding>();
    }

    public Case(
        Guid transactionId,
        Guid clientId,
        Guid accountId,
        Severity initialSeverity,
        Guid? analystId = null)
    {
        Id = Guid.NewGuid();
        TransactionId = transactionId;
        ClientId = clientId;
        AccountId = accountId;
        Severity = initialSeverity;
        AnalystId = analystId;
        Status = analystId.HasValue ? CaseStatus.UnderReview : CaseStatus.New;
        OpenedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        _findings = new List<CaseFinding>();
    }

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

    private readonly List<CaseFinding> _findings = new();
    public IReadOnlyCollection<CaseFinding> Findings => _findings.AsReadOnly();

    public void AssignAnalyst(Guid analystId)
    {
        if (Status == CaseStatus.Resolved)
            throw new InvalidOperationException("Cannot assign analyst to a resolved case");

        AnalystId = analystId;
        if (Status == CaseStatus.New)
            Status = CaseStatus.UnderReview;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void ReassignAnalyst(Guid newAnalystId)
    {
        if (Status == CaseStatus.Resolved)
            throw new InvalidOperationException("Cannot reassign analyst to a resolved case");
        if (!AnalystId.HasValue)
            throw new InvalidOperationException("Case has no analyst assigned yet. Use AssignAnalyst instead.");

        AnalystId = newAnalystId;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Resolve(CaseDecision decision)
    {
        if (Status == CaseStatus.Resolved)
            throw new InvalidOperationException("Case is already resolved");

        Status = CaseStatus.Resolved;
        Decision = decision;
        ResolvedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Reopen()
    {
        if (Status != CaseStatus.Resolved)
            throw new InvalidOperationException($"Cannot reopen a case with status {Status}. Expected Resolved.");

        Status = CaseStatus.UnderReview;
        Decision = null;
        ResolvedAtUtc = null;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void AddFinding(CaseFinding finding)
    {
        if (finding == null)
            throw new ArgumentNullException(nameof(finding));
        if (finding.CaseId != Id)
            throw new InvalidOperationException("Finding does not belong to this case");

        _findings.Add(finding);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void EscalateSeverity(Severity newSeverity)
    {
        if (newSeverity <= Severity)
            throw new InvalidOperationException($"Cannot escalate to {newSeverity}. Current severity is {Severity}.");

        Severity = newSeverity;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}

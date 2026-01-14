using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class CaseFinding
{
    private CaseFinding() { }

    public CaseFinding(
        Guid caseId,
        Guid ruleId,
        RuleType ruleType,
        Severity severity,
        JsonDocument evidenceJson)
    {
        if (evidenceJson == null)
            throw new ArgumentNullException(nameof(evidenceJson));

        Id = Guid.NewGuid();
        CaseId = caseId;
        RuleId = ruleId;
        RuleType = ruleType;
        Severity = severity;
        EvidenceJson = evidenceJson;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid CaseId { get; private set; }
    public Case Case { get; set; } = null!; 

    public Guid RuleId { get; private set; }
    public ComplianceRule Rule { get; set; } = null!; 

    public RuleType RuleType { get; private set; }
    public Severity Severity { get; private set; }

    public JsonDocument EvidenceJson { get; private set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; private set; }
}

using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class CaseFinding
{
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

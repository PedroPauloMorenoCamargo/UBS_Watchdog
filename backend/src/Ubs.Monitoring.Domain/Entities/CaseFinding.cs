using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class CaseFinding
{
    public Guid Id { get; set; }

    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    public Guid RuleId { get; set; }
    public ComplianceRule Rule { get; set; } = null!;

    public RuleType RuleType { get; set; }
    public Severity Severity { get; set; }

    public JsonDocument EvidenceJson { get; set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; set; }
}

using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class ComplianceRule
{
    public Guid Id { get; set; }
    public RuleType RuleType { get; set; }
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
    public Severity Severity { get; set; }
    public string? Scope { get; set; }
    public JsonDocument ParametersJson { get; set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ICollection<CaseFinding> CaseFindings { get; set; } = new List<CaseFinding>();
}

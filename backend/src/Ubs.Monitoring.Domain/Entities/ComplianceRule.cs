using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class ComplianceRule
{
    public Guid Id { get; private set; }
    public RuleType RuleType { get; private set; } 
    public string Name { get; set; } = null!;
    public bool IsActive { get; private set; } 
    public Severity Severity { get; private set; } 
    public string? Scope { get; set; }
    public JsonDocument ParametersJson { get; set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public ICollection<CaseFinding> CaseFindings { get; set; } = new List<CaseFinding>();
}

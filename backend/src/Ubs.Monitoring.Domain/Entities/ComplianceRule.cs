using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class ComplianceRule
{
    private ComplianceRule() { }

    public ComplianceRule(
        RuleType ruleType,
        string name,
        Severity severity,
        JsonDocument parametersJson,
        string? scope = null,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Rule name is required", nameof(name));
        if (parametersJson == null)
            throw new ArgumentNullException(nameof(parametersJson));

        Id = Guid.NewGuid();
        RuleType = ruleType;
        Name = name;
        Severity = severity;
        ParametersJson = parametersJson;
        Scope = scope;
        IsActive = isActive;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public RuleType RuleType { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public Severity Severity { get; private set; }
    public string? Scope { get; private set; }
    public JsonDocument ParametersJson { get; private set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public ICollection<CaseFinding> CaseFindings { get; set; } = new List<CaseFinding>();

    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("Rule is already active");

        IsActive = true;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("Rule is already inactive");

        IsActive = false;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateParameters(JsonDocument newParameters)
    {
        if (newParameters == null)
            throw new ArgumentNullException(nameof(newParameters));

        ParametersJson = newParameters;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateSeverity(Severity newSeverity)
    {
        Severity = newSeverity;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateScope(string? newScope)
    {
        Scope = newScope;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}

using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public sealed class ComplianceRule
{
    private ComplianceRule() { } // EF Core

    public ComplianceRule(
        string code,
        RuleType ruleType,
        string name,
        Severity severity,
        string parametersJson,
        string? scope = null,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Rule code is required.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Rule name is required.", nameof(name));
        if (name.Length > 150)
            throw new ArgumentException("Rule name max length is 150.", nameof(name));

        if (scope is not null && scope is not ("PerClient" or "PerAccount"))
            throw new ArgumentException("Scope must be null, 'PerClient' or 'PerAccount'.", nameof(scope));

        EnsureValidJson(parametersJson);

        Id = Guid.NewGuid();
        Code = code.Trim();
        RuleType = ruleType;
        Name = name.Trim();
        Severity = severity;
        ParametersJson = parametersJson;
        Scope = scope;
        IsActive = isActive;

        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = null!;

    public RuleType RuleType { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public Severity Severity { get; private set; }

    public string? Scope { get; private set; }

    public string ParametersJson { get; private set; } = null!;

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public ICollection<CaseFinding> CaseFindings { get; private set; } = new List<CaseFinding>();

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Rule name is required.", nameof(newName));

        Name = newName.Trim();
        Touch();
    }

    public void SetActive(bool active)
    {
        // Best practice: idempotent
        if (IsActive == active) return;
        IsActive = active;
        Touch();
    }

    public void UpdateSeverity(Severity newSeverity)
    {
        if (Severity == newSeverity) return;
        Severity = newSeverity;
        Touch();
    }

    public void UpdateScope(string? newScope)
    {
        if (newScope is not null && newScope is not ("PerClient" or "PerAccount"))
            throw new ArgumentException("Scope must be null, 'PerClient' or 'PerAccount'.", nameof(newScope));

        if (Scope == newScope) return;
        Scope = newScope;
        Touch();
    }

    public void UpdateParametersJson(string newParametersJson)
    {
        EnsureValidJson(newParametersJson);
        ParametersJson = newParametersJson;
        Touch();
    }

    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;

    private static void EnsureValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("ParametersJson is required.", nameof(json));

        try
        {
            using var _ = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("ParametersJson must be valid JSON.", nameof(json), ex);
        }
    }
}

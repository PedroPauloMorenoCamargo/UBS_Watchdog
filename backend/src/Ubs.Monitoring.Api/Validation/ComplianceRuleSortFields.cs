namespace Ubs.Monitoring.Api.Validation;

public static class ComplianceRuleSortFields
{
    public const string Name = "Name";
    public const string RuleType = "RuleType";
    public const string Severity = "Severity";
    public const string CreatedAtUtc = "CreatedAtUtc";
    public const string UpdatedAtUtc = "UpdatedAtUtc";

    private static readonly HashSet<string> Allowed =
        new(StringComparer.OrdinalIgnoreCase)
        {
            Name,
            RuleType,
            Severity,
            CreatedAtUtc,
            UpdatedAtUtc
        };

    public static bool IsValid(string value)
        => !string.IsNullOrWhiteSpace(value) && Allowed.Contains(value);
}

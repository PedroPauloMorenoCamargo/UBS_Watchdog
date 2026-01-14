namespace Ubs.Monitoring.Api.Validation.Sorting;

public static class ComplianceRuleSortFields
{
    private static readonly HashSet<string> Allowed =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Name",
            "Code",
            "RuleType",
            "Severity",
            "IsActive",
            "CreatedAtUtc",
            "UpdatedAtUtc",
        };

    public static bool IsValid(string field) => Allowed.Contains(field);

    public static string AllowedList() => string.Join(", ", Allowed.OrderBy(x => x));
}

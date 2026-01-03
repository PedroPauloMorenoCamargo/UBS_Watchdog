namespace Ubs.Monitoring.Api.Validation;

public static class ComplianceRuleScopes
{
    private static readonly HashSet<string> Allowed =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "PerClient",
            "PerAccount"
        };

    public static bool IsValid(string value)
        => Allowed.Contains(value);
}

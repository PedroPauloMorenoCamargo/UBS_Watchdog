public static class ComplianceRuleSortFields
{
    private static readonly HashSet<string> Allowed =
    [
        "name",
        "ruletype",
        "severity",
        "createdatutc",
        "updatedatutc"
    ];

    public static bool IsValid(string? value)  => string.IsNullOrWhiteSpace(value) || Allowed.Contains(value.Trim().ToLowerInvariant());

    public static string AllowedList() => string.Join(", ", Allowed);
}

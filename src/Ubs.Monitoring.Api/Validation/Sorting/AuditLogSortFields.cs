namespace Ubs.Monitoring.Api.Validation.Sorting;

public static class AuditLogSortFields
{
    private static readonly HashSet<string> Allowed =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "PerformedAtUtc",
            "EntityType",
            "EntityId",
            "Action",
            "PerformedByAnalystId",
            "CorrelationId"
        };

    public static bool IsValid(string field) => Allowed.Contains(field);

    public static string AllowedList() => string.Join(", ", Allowed.OrderBy(x => x));
}

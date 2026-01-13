namespace Ubs.Monitoring.Api.Validation;

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

    public static bool IsValid(string? sortBy)
        => !string.IsNullOrWhiteSpace(sortBy) && Allowed.Contains(sortBy.Trim());
}

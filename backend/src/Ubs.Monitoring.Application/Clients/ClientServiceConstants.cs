namespace Ubs.Monitoring.Application.Clients;

/// <summary>
/// Constants specific to ClientService operations.
/// For pagination defaults (MaxPageSize, DefaultPageSize), use PaginationDefaults from Common.Pagination.
/// </summary>
internal static class ClientServiceConstants
{
    /// <summary>
    /// Optimal batch size for bulk import operations.
    /// Based on EF Core performance recommendations.
    /// </summary>
    public const int ImportBatchSize = 500;

    /// <summary>
    /// Line number offset for import error reporting.
    /// Accounts for 0-based array index + header row.
    /// </summary>
    public const int ImportLineNumberOffset = 2;
}

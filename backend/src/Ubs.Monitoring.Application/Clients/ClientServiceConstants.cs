namespace Ubs.Monitoring.Application.Clients;

/// <summary>
/// Constants used by the ClientService for validation and configuration.
/// </summary>
internal static class ClientServiceConstants
{
    /// <summary>
    /// Maximum number of items allowed per page in paginated queries.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Default number of items per page when not specified.
    /// </summary>
    public const int DefaultPageSize = 20;

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

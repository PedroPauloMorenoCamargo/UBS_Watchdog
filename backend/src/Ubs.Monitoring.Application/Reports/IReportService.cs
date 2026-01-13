namespace Ubs.Monitoring.Application.Reports;

/// <summary>
/// Service interface for generating reports and analytics.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Generates a comprehensive report for a specific client.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="startDate">Start date of the report period (inclusive). If null, uses 30 days ago.</param>
    /// <param name="endDate">End date of the report period (inclusive). If null, uses today.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Client report data if client exists; otherwise, null.</returns>
    Task<ClientReportDto?> GetClientReportAsync(
        Guid clientId,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken ct);

    /// <summary>
    /// Generates a system-wide report across all clients.
    /// </summary>
    /// <param name="startDate">Start date of the report period (inclusive). If null, uses 30 days ago.</param>
    /// <param name="endDate">End date of the report period (inclusive). If null, uses today.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>System-wide report data.</returns>
    Task<SystemReportDto> GetSystemReportAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken ct);

    /// <summary>
    /// Generates CSV export for client report.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="startDate">Start date of the report period (inclusive).</param>
    /// <param name="endDate">End date of the report period (inclusive).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>CSV string if client exists; otherwise, null.</returns>
    Task<string?> GenerateClientReportCsvAsync(
        Guid clientId,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken ct);

    /// <summary>
    /// Generates CSV export for system report.
    /// </summary>
    /// <param name="startDate">Start date of the report period (inclusive).</param>
    /// <param name="endDate">End date of the report period (inclusive).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>CSV string.</returns>
    Task<string> GenerateSystemReportCsvAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken ct);
}

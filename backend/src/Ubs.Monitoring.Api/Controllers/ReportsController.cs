using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Application.Reports;

namespace Ubs.Monitoring.Api.Controllers;

/// <summary>
/// Manages report generation and export operations.
/// </summary>
[Authorize]
[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportService reportService,
        ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a comprehensive report for a specific client.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="startDate">Start date of the report period (inclusive). Defaults to 30 days ago if not provided.</param>
    /// <param name="endDate">End date of the report period (inclusive). Defaults to today if not provided.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Client report with transaction metrics, case metrics, and chart data.</returns>
    /// <response code="200">Returns the client report successfully.</response>
    /// <response code="404">Client not found.</response>
    [HttpGet("client/{clientId:guid}")]
    [ProducesResponseType(typeof(ClientReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientReportDto>> GetClientReport(
        Guid clientId,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        CancellationToken ct)
    {
        _logger.LogInformation("Fetching client report for {ClientId}", clientId);

        var report = await _reportService.GetClientReportAsync(clientId, startDate, endDate, ct);

        if (report is null)
        {
            return Problem(
                title: "Client not found",
                detail: $"No client found with ID '{clientId}'.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(report);
    }

    /// <summary>
    /// Retrieves a system-wide report across all clients.
    /// </summary>
    /// <param name="startDate">Start date of the report period (inclusive). Defaults to 30 days ago if not provided.</param>
    /// <param name="endDate">End date of the report period (inclusive). Defaults to today if not provided.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>System report with aggregated metrics and chart data.</returns>
    /// <response code="200">Returns the system report successfully.</response>
    [HttpGet("system")]
    [ProducesResponseType(typeof(SystemReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SystemReportDto>> GetSystemReport(
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        CancellationToken ct)
    {
        _logger.LogInformation("Fetching system report");

        var report = await _reportService.GetSystemReportAsync(startDate, endDate, ct);

        return Ok(report);
    }

    /// <summary>
    /// Exports a client report as CSV file.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="startDate">Start date of the report period (inclusive). Defaults to 30 days ago if not provided.</param>
    /// <param name="endDate">End date of the report period (inclusive). Defaults to today if not provided.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>CSV file containing the client report.</returns>
    /// <response code="200">Returns the CSV file successfully.</response>
    /// <response code="404">Client not found.</response>
    [HttpGet("client/{clientId:guid}/export/csv")]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportClientReportCsv(
        Guid clientId,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        CancellationToken ct)
    {
        _logger.LogInformation("Exporting client report CSV for {ClientId}", clientId);

        var csv = await _reportService.GenerateClientReportCsvAsync(clientId, startDate, endDate, ct);

        if (csv is null)
        {
            return Problem(
                title: "Client not found",
                detail: $"No client found with ID '{clientId}'.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        var fileName = $"client_report_{clientId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        var bytes = Encoding.UTF8.GetBytes(csv);

        return File(bytes, "text/csv", fileName);
    }

    /// <summary>
    /// Exports a system report as CSV file.
    /// </summary>
    /// <param name="startDate">Start date of the report period (inclusive). Defaults to 30 days ago if not provided.</param>
    /// <param name="endDate">End date of the report period (inclusive). Defaults to today if not provided.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>CSV file containing the system report.</returns>
    /// <response code="200">Returns the CSV file successfully.</response>
    [HttpGet("system/export/csv")]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportSystemReportCsv(
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        CancellationToken ct)
    {
        _logger.LogInformation("Exporting system report CSV");

        var csv = await _reportService.GenerateSystemReportCsvAsync(startDate, endDate, ct);

        var fileName = $"system_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        var bytes = Encoding.UTF8.GetBytes(csv);

        return File(bytes, "text/csv", fileName);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Api.Contracts;
using Ubs.Monitoring.Api.Mappers.AuditLogs;
using Ubs.Monitoring.Application.AuditLogs;

namespace Ubs.Monitoring.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/audit-logs")]
[Produces("application/json", "application/problem+json")]
public sealed class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogsController(IAuditLogService service) => _service = service;

    /// <summary>
    /// Searches audit logs using pagination, sorting, and optional filtering criteria.
    /// </summary>
    /// <param name="req">
    /// The search request containing pagination, sorting, and filter parameters.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// A paged collection of audit log entries matching the provided criteria.
    /// </returns>
    /// <response code="200">Returns the paged list of audit logs.</response>
    /// <response code="400">Invalid query parameters.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<AuditLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search([FromQuery] SearchAuditLogsRequest req, CancellationToken ct)
    {
        var query = AuditLogContractMapper.ToQuery(req);
        var result = await _service.SearchAsync(query, ct);

        return Ok(AuditLogContractMapper.ToPagedResponse(result));
    }

    /// <summary>
    /// Retrieves a single audit log entry by its unique identifier.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the audit log entry.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// The audit log entry if found.
    /// </returns>
    /// <response code="200">Returns the audit log entry.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Audit log entry not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AuditLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var dto = await _service.GetByIdAsync(id, ct);
        if (dto is null)
        {
            return Problem(
                title: "Audit log not found",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(AuditLogContractMapper.ToResponse(dto));
    }
}

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
    /// Search audit logs with pagination, filtering and sorting.
    /// </summary>
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
    /// Get a single audit log by ID.
    /// </summary>
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Api.Contracts.AuditLogs;
using Ubs.Monitoring.Api.Extensions;
using Ubs.Monitoring.Api.Validation;
using Ubs.Monitoring.Application.AuditLogs;
using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/audit-logs")]
[Produces("application/json")]
public sealed class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogsController(IAuditLogService service) => _service = service;

    /// <summary>
    /// Search audit logs with pagination, filtering and sorting.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuditLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] int page = PaginationDefaults.DefaultPage,
        [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize,
        [FromQuery] string? entityType = null,
        [FromQuery] string? entityId = null,
        [FromQuery] AuditAction? action = null,
        [FromQuery] Guid? performedByAnalystId = null,
        [FromQuery] string? correlationId = null,
        [FromQuery] DateTimeOffset? fromUtc = null,
        [FromQuery] DateTimeOffset? toUtc = null,
        [FromQuery] string sortBy = "PerformedAtUtc",
        [FromQuery] string sortDir = "desc",
        CancellationToken ct = default)
    {
        if (!AuditLogSortFields.IsValid(sortBy))
            return Problem(title: "Invalid sort field", statusCode: StatusCodes.Status400BadRequest);

        if (!string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase))
            return Problem(title: "Invalid sort direction", statusCode: StatusCodes.Status400BadRequest);

        if (fromUtc is not null && toUtc is not null && fromUtc > toUtc)
            return Problem(title: "Invalid date range", detail: "fromUtc must be <= toUtc.", statusCode: StatusCodes.Status400BadRequest);

        var result = await _service.SearchAsync(new AuditLogQuery
        {
            Page = new PageRequest
            {
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDir = sortDir
            },
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            PerformedByAnalystId = performedByAnalystId,
            CorrelationId = correlationId,
            FromUtc = fromUtc,
            ToUtc = toUtc
        }, ct);

        return Ok(result.Map(ToResponse));
    }

    /// <summary>
    /// Get a single audit log by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AuditLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var log = await _service.GetByIdAsync(id, ct);
        if (log is null)
        {
            return Problem(
                title: "Audit log not found",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(ToResponse(log));
    }

    private static AuditLogResponse ToResponse(AuditLogDto x) =>
        new(
            x.Id,
            x.EntityType,
            x.EntityId,
            x.Action,
            x.PerformedByAnalystId,
            x.CorrelationId,
            x.Before,
            x.After,
            x.PerformedAtUtc
        );
}

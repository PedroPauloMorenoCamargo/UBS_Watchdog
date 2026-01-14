using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Application.Cases;
using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Api.Controllers;

/// <summary>
/// Controller for managing compliance case operations.
/// </summary>
[Authorize]
[ApiController]
[Route("api/cases")]
public sealed class CasesController : ControllerBase
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;
    private const int MinPage = 1;

    private readonly ICaseService _caseService;

    public CasesController(ICaseService caseService)
    {
        _caseService = caseService;
    }

    /// <summary>
    /// Retrieves a paginated and filtered list of cases.
    /// </summary>
    /// <param name="clientId">Optional filter by client ID.</param>
    /// <param name="accountId">Optional filter by account ID.</param>
    /// <param name="transactionId">Optional filter by transaction ID.</param>
    /// <param name="analystId">Optional filter by assigned analyst ID.</param>
    /// <param name="status">Optional filter by case status (New, UnderReview, Resolved).</param>
    /// <param name="decision">Optional filter by decision (Fraudulent, NotFraudulent, Inconclusive).</param>
    /// <param name="severity">Optional filter by severity (Low, Medium, High, Critical).</param>
    /// <param name="openedFrom">Optional filter for cases opened on or after this date.</param>
    /// <param name="openedTo">Optional filter for cases opened on or before this date.</param>
    /// <param name="resolvedFrom">Optional filter for cases resolved on or after this date.</param>
    /// <param name="resolvedTo">Optional filter for cases resolved on or before this date.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100).</param>
    /// <param name="sortBy">Field to sort by (openedAtUtc, updatedAtUtc, resolvedAtUtc, severity, status, clientName).</param>
    /// <param name="sortDesc">Sort descending (default: true).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated list of cases.</returns>
    /// <response code="200">Returns the paginated list of cases.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CaseResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<CaseResponseDto>>> GetCases(
        [FromQuery] Guid? clientId = null,
        [FromQuery] Guid? accountId = null,
        [FromQuery] Guid? transactionId = null,
        [FromQuery] Guid? analystId = null,
        [FromQuery] CaseStatus? status = null,
        [FromQuery] CaseDecision? decision = null,
        [FromQuery] Severity? severity = null,
        [FromQuery] DateTimeOffset? openedFrom = null,
        [FromQuery] DateTimeOffset? openedTo = null,
        [FromQuery] DateTimeOffset? resolvedFrom = null,
        [FromQuery] DateTimeOffset? resolvedTo = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = true,
        CancellationToken ct = default)
    {
        // Validate pagination
        if (page < MinPage) page = MinPage;
        if (pageSize < 1) pageSize = DefaultPageSize;
        if (pageSize > MaxPageSize) pageSize = MaxPageSize;

        var filter = new CaseFilterRequest(
            ClientId: clientId,
            AccountId: accountId,
            TransactionId: transactionId,
            AnalystId: analystId,
            Status: status,
            Decision: decision,
            Severity: severity,
            OpenedFrom: openedFrom,
            OpenedTo: openedTo,
            ResolvedFrom: resolvedFrom,
            ResolvedTo: resolvedTo,
            Page: page,
            PageSize: pageSize,
            SortBy: sortBy,
            SortDescending: sortDesc
        );

        var result = await _caseService.GetPagedAsync(filter, ct);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves detailed information about a specific case.
    /// </summary>
    /// <param name="id">The unique identifier of the case.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Detailed case information including transaction, client, account, analyst, and findings.</returns>
    /// <response code="200">Returns the case details.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Case not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CaseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CaseDetailDto>> GetCaseById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _caseService.GetByIdAsync(id, ct);

        if (result is null)
        {
            return Problem(
                title: "Case not found",
                detail: $"No case found with ID: {id}",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(result);
    }

    /// <summary>
    /// Retrieves all findings for a specific case.
    /// </summary>
    /// <param name="caseId">The unique identifier of the case.</param>
    /// <param name="sortBy">Field to sort by (severity, ruleType, ruleName, createdAtUtc).</param>
    /// <param name="sortDesc">Sort descending (default: true).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of findings for the case.</returns>
    /// <response code="200">Returns the list of findings.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Case not found.</response>
    [HttpGet("{caseId:guid}/findings")]
    [ProducesResponseType(typeof(IReadOnlyList<CaseFindingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CaseFindingDto>>> GetCaseFindings(
        [FromRoute] Guid caseId,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = true,
        CancellationToken ct = default)
    {
        var filter = new CaseFindingFilterRequest(
            SortBy: sortBy,
            SortDescending: sortDesc
        );

        var result = await _caseService.GetFindingsAsync(caseId, filter, ct);

        if (result is null)
        {
            return Problem(
                title: "Case not found",
                detail: $"No case found with ID: {caseId}",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(result);
    }

    /// <summary>
    /// Updates the case workflow (status, decision, analyst assignment).
    /// </summary>
    /// <param name="id">The unique identifier of the case.</param>
    /// <param name="request">The update request containing status, decision, and/or analyst assignment.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated case data.</returns>
    /// <response code="200">Case updated successfully.</response>
    /// <response code="400">Invalid request or workflow transition not allowed.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Case or analyst not found.</response>
    /// <remarks>
    /// Workflow rules:
    ///
    /// - **New → UnderReview**: Requires an analyst to be assigned first.
    /// - **UnderReview → Resolved**: Requires a decision (Fraudulent, NotFraudulent, or Inconclusive).
    /// - **Resolved → UnderReview**: Case can be reopened (decision is cleared).
    /// - **Cannot transition back to New** from any other status.
    ///
    /// If a decision is provided without an explicit status, the case will automatically be resolved.
    /// </remarks>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(CaseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CaseResponseDto>> UpdateCase(
        [FromRoute] Guid id,
        [FromBody] UpdateCaseRequest request,
        CancellationToken ct)
    {
        var (result, errorMessage) = await _caseService.UpdateAsync(id, request, ct);

        if (result is null)
        {
            if (errorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Problem(
                    title: "Not found",
                    detail: errorMessage,
                    statusCode: StatusCodes.Status404NotFound
                );
            }

            return Problem(
                title: "Cannot update case",
                detail: errorMessage ?? "Failed to update case.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        return Ok(result);
    }

    /// <summary>
    /// Assigns the case to the currently authenticated analyst.
    /// </summary>
    /// <param name="id">The unique identifier of the case.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated case data with the analyst assigned.</returns>
    /// <response code="200">Case assigned successfully.</response>
    /// <response code="400">Cannot assign case (e.g., case is already resolved).</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Case not found.</response>
    /// <remarks>
    /// This endpoint assigns the case to the analyst making the request.
    ///
    /// - If the case is in **New** status, it transitions to **UnderReview**.
    /// - If the case is already **UnderReview**, the analyst is reassigned.
    /// - **Resolved** cases cannot be assigned.
    /// </remarks>
    [HttpPost("{id:guid}/assign-to-me")]
    [ProducesResponseType(typeof(CaseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CaseResponseDto>> AssignToMe(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        if (!TryGetAnalystId(out var analystId))
        {
            return Problem(
                title: "Unauthorized",
                detail: "Unable to identify the authenticated analyst.",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }

        var (result, errorMessage) = await _caseService.AssignToAnalystAsync(id, analystId, ct);

        if (result is null)
        {
            if (errorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Problem(
                    title: "Not found",
                    detail: errorMessage,
                    statusCode: StatusCodes.Status404NotFound
                );
            }

            return Problem(
                title: "Cannot assign case",
                detail: errorMessage ?? "Failed to assign case.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        return Ok(result);
    }

    /// <summary>
    /// Extracts the authenticated analyst's ID from JWT claims.
    /// </summary>
    private bool TryGetAnalystId(out Guid id)
    {
        var raw =
            User.FindFirstValue("sub") ??
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(raw, out id);
    }
}

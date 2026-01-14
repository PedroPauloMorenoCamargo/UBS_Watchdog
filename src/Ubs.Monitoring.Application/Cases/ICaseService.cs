using Ubs.Monitoring.Application.Common.Pagination;

namespace Ubs.Monitoring.Application.Cases;

/// <summary>
/// Service interface for case business operations.
/// </summary>
public interface ICaseService
{
    /// <summary>
    /// Retrieves a paginated list of cases with filtering and sorting.
    /// </summary>
    /// <param name="filter">The filter and pagination parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated result containing case DTOs and metadata.</returns>
    Task<PagedResult<CaseResponseDto>> GetPagedAsync(
        CaseFilterRequest filter,
        CancellationToken ct);

    /// <summary>
    /// Retrieves detailed information about a specific case.
    /// </summary>
    /// <param name="caseId">The unique identifier of the case.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The case details if found; otherwise, null.</returns>
    Task<CaseDetailDto?> GetByIdAsync(Guid caseId, CancellationToken ct);

    /// <summary>
    /// Retrieves all findings for a specific case.
    /// </summary>
    /// <param name="caseId">The unique identifier of the case.</param>
    /// <param name="filter">The sorting parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of findings if case exists; otherwise, null.</returns>
    Task<IReadOnlyList<CaseFindingDto>?> GetFindingsAsync(
        Guid caseId,
        CaseFindingFilterRequest filter,
        CancellationToken ct);

    /// <summary>
    /// Updates the case workflow (status, decision, analyst assignment).
    /// </summary>
    /// <param name="caseId">The unique identifier of the case.</param>
    /// <param name="request">The update request data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the updated case data and error message.
    /// If successful, Result contains the case data and ErrorMessage is null.
    /// If failed, Result is null and ErrorMessage contains the validation error.
    /// </returns>
    Task<(CaseResponseDto? Result, string? ErrorMessage)> UpdateAsync(
        Guid caseId,
        UpdateCaseRequest request,
        CancellationToken ct);

    /// <summary>
    /// Assigns the case to the specified analyst.
    /// </summary>
    /// <param name="caseId">The unique identifier of the case.</param>
    /// <param name="analystId">The unique identifier of the analyst.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the updated case data and error message.
    /// If successful, Result contains the case data and ErrorMessage is null.
    /// If failed, Result is null and ErrorMessage contains the validation error.
    /// </returns>
    Task<(CaseResponseDto? Result, string? ErrorMessage)> AssignToAnalystAsync(
        Guid caseId,
        Guid analystId,
        CancellationToken ct);
}

using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.Cases;

/// <summary>
/// Repository interface for Case aggregate operations.
/// </summary>
public interface ICaseRepository
{
    /// <summary>
    /// Retrieves a case by its ID without tracking.
    /// </summary>
    Task<Case?> GetByIdAsync(Guid caseId, CancellationToken ct);

    /// <summary>
    /// Retrieves a case by its ID with all related entities for detail view.
    /// </summary>
    Task<Case?> GetByIdWithDetailsAsync(Guid caseId, CancellationToken ct);

    /// <summary>
    /// Retrieves a case by its ID with tracking for updates.
    /// </summary>
    Task<Case?> GetForUpdateAsync(Guid caseId, CancellationToken ct);

    /// <summary>
    /// Retrieves a case by transaction ID.
    /// </summary>
    Task<Case?> GetByTransactionIdAsync(Guid transactionId, CancellationToken ct);

    /// <summary>
    /// Retrieves a paginated list of cases with filtering and sorting.
    /// </summary>
    Task<(IReadOnlyList<Case> Items, int TotalCount)> GetPagedAsync(
        CaseFilterRequest filter,
        CancellationToken ct);

    /// <summary>
    /// Retrieves all findings for a case with sorting.
    /// </summary>
    Task<IReadOnlyList<CaseFinding>> GetFindingsByCaseIdAsync(
        Guid caseId,
        CaseFindingFilterRequest filter,
        CancellationToken ct);

    /// <summary>
    /// Checks if a case exists by ID.
    /// </summary>
    Task<bool> ExistsAsync(Guid caseId, CancellationToken ct);

    /// <summary>
    /// Adds a new case to the context.
    /// </summary>
    void Add(Case caseEntity);

    /// <summary>
    /// Adds a new case finding to the context.
    /// </summary>
    void AddFinding(CaseFinding finding);

    /// <summary>
    /// Persists all changes to the database.
    /// </summary>
    Task SaveChangesAsync(CancellationToken ct);
}

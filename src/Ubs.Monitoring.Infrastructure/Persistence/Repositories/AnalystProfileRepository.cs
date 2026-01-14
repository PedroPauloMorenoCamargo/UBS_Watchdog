using Microsoft.EntityFrameworkCore;
using Ubs.Monitoring.Application.Analysts;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Infrastructure.Persistence;

namespace Ubs.Monitoring.Infrastructure.Persistence.Repositories;

public sealed class AnalystProfileRepository : IAnalystProfileRepository
{
    private readonly AppDbContext _db;
    public AnalystProfileRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Retrieves an analyst entity for update operations.
    /// </summary>
    /// <remarks>
    ///  Do not use this method for read-only operations exposed to the API.
    /// </remarks>
    /// <param name="analystId">
    /// The unique identifier of the analyst to retrieve.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the database query.
    /// </param>
    /// <returns>
    /// The tracked <see cref="Analyst"/> entity if found; otherwise, <c>null</c>.
    /// </returns>
    public Task<Analyst?> GetForUpdateAsync(Guid analystId, CancellationToken ct) => _db.Analysts.FirstOrDefaultAsync(a => a.Id == analystId, ct);

    /// <summary>
    /// Persists all pending changes made to tracked entities.
    /// </summary>
    /// <param name="ct">
    /// Cancellation token used to cancel the save operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous save operation.
    /// </returns>
    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

    /// <summary>
    /// Retrieves a read-only analyst profile.
    /// </summary>
    /// <remarks>
    /// This method is intended for query/read scenarios.
    /// </remarks>
    /// <param name="analystId">
    /// The unique identifier of the analyst whose profile should be retrieved.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the database query.
    /// </param>
    /// <returns>
    /// An <see cref="AnalystProfileDto"/> containing the analyst's profile information if the analyst exists; otherwise, <c>null</c>.
    /// </returns>
    public Task<AnalystProfileDto?> GetProfileByIdAsync(Guid analystId, CancellationToken ct)
        => _db.Analysts
            .AsNoTracking()
            .Where(a => a.Id == analystId)
            .Select(a => new AnalystProfileDto(
                a.Id,
                a.CorporateEmail,
                a.FullName,
                a.PhoneNumber,
                a.ProfilePictureBase64,
                a.CreatedAtUtc
            ))
            .FirstOrDefaultAsync(ct);
}

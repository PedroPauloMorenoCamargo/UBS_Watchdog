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
    /// <param name="analystId">
    /// The unique identifier of the analyst to retrieve.
    /// </param>
    /// <param name="ct">
    /// A cancellation token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The tracked <see cref="Analyst"/> entity if found; otherwise, <c>null</c>.
    /// </returns>
    public Task<Analyst?> GetForUpdateAsync(Guid analystId, CancellationToken ct)
        => _db.Analysts.FirstOrDefaultAsync(a => a.Id == analystId, ct);
    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <param name="ct">
    /// A cancellation token used to cancel the save operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous save operation.
    /// </returns>
    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}

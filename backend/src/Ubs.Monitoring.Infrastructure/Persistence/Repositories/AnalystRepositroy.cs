using Microsoft.EntityFrameworkCore;
using Ubs.Monitoring.Application.Auth;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Infrastructure.Persistence;

namespace Ubs.Monitoring.Infrastructure.Persistence.Repositories;

public sealed class AnalystRepository : IAnalystRepository
{
    private readonly AppDbContext _db;

    public AnalystRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Retrieves an analyst by its unique identifier.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the analyst.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The matching <see cref="Analyst"/> if found; otherwise, <c>null</c>.
    /// </returns>
    public Task<Analyst?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Analysts
              .AsNoTracking()
              .FirstOrDefaultAsync(a => a.Id == id, ct);

    /// <summary>
    /// Retrieves an analyst by its normalized corporate email address.
    /// </summary>
    /// <param name="normalizedEmail">
    /// The normalized corporate email used to identify the analyst.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The matching <see cref="Analyst"/> if found; otherwise, <c>null</c>.
    /// </returns>
    public Task<Analyst?> GetByEmailAsync(string normalizedEmail, CancellationToken ct)
        => _db.Analysts
              .AsNoTracking()
              .FirstOrDefaultAsync(a => a.CorporateEmail == normalizedEmail, ct);
}

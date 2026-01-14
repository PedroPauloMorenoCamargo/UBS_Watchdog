using Microsoft.EntityFrameworkCore;
using Ubs.Monitoring.Application.Analysts;
using Ubs.Monitoring.Application.Auth;
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
    /// Retrieves authentication-related data for an analyst by normalized email address.
    /// </summary>
    /// <param name="normalizedEmail">
    /// The analyst’s email address normalized to a canonical form (e.g. trimmed and lowercased).
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// An <see cref="AnalystAuthDto"/> containing authentication data if an analyst  with the specified email exists; otherwise, <c>null</c>.
    /// </returns>
    public Task<AnalystAuthDto?> GetAuthByEmailAsync(string normalizedEmail, CancellationToken ct)
        => _db.Analysts
            .AsNoTracking()
            .Where(a => a.CorporateEmail == normalizedEmail)
            .Select(a => new AnalystAuthDto(
                a.Id,
                a.CorporateEmail,
                a.FullName,
                a.PasswordHash,
                a.PhoneNumber,
                a.ProfilePictureBase64,
                a.CreatedAtUtc
            ))
            .FirstOrDefaultAsync(ct);
    /// <summary>
    /// Retrieves the public analyst profile by unique identifier.
    /// </summary>
    /// <param name="analystId">
    /// The unique identifier of the analyst.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// An <see cref="AnalystProfileDto"/> if the analyst exists; otherwise, <c>null</c>.
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

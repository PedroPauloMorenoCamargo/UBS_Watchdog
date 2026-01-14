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

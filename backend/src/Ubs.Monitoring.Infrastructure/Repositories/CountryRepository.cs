using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ubs.Monitoring.Application.Countries;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Infrastructure.Persistence;

namespace Ubs.Monitoring.Infrastructure.Repositories;

public sealed class CountryRepository : ICountryRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<CountryRepository> _logger;

    public CountryRepository(AppDbContext db, ILogger<CountryRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving all countries from database");

        var countries = await _db.Countries
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        _logger.LogInformation("Retrieved {Count} countries", countries.Count);

        return countries;
    }

    public async Task<Country?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(code);

        _logger.LogDebug("Retrieving country with code {Code} for read", code);

        var country = await _db.Countries
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), ct);

        if (country == null)
            _logger.LogWarning("Country with code {Code} not found", code);

        return country;
    }

    public async Task<Country?> GetByCodeForUpdateAsync(string code, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(code);

        _logger.LogDebug("Retrieving country with code {Code} for update", code);

        // WITHOUT AsNoTracking() to enable tracking for updates
        var country = await _db.Countries
            .FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), ct);

        if (country == null)
            _logger.LogWarning("Country with code {Code} not found", code);

        return country;
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(code);

        _logger.LogDebug("Checking if country with code {Code} exists", code);

        return await _db.Countries
            .AsNoTracking()
            .AnyAsync(c => c.Code == code.ToUpperInvariant(), ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Saving country changes to database");

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Country changes saved successfully");
    }
}

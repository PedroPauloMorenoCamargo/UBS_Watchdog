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

    public async Task<HashSet<string>> GetExistingCodesAsync(IEnumerable<string> codes, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(codes);

        var normalizedCodes = codes
            .Select(c => c.Trim().ToUpperInvariant())
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .ToList();

        if (normalizedCodes.Count == 0)
        {
            _logger.LogDebug("No valid country codes provided for batch check");
            return new HashSet<string>();
        }

        _logger.LogDebug("Checking existence of {Count} country codes in a single query", normalizedCodes.Count);

        var existingCodes = await _db.Countries
            .AsNoTracking()
            .Where(c => normalizedCodes.Contains(c.Code))
            .Select(c => c.Code)
            .ToListAsync(ct);

        _logger.LogDebug("Found {FoundCount} out of {TotalCount} country codes", existingCodes.Count, normalizedCodes.Count);

        return existingCodes.ToHashSet();
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Saving country changes to database");

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Country changes saved successfully");
    }
}

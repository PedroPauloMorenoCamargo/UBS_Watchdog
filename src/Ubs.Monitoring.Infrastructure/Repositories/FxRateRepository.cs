using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ubs.Monitoring.Application.FxRates;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Infrastructure.Persistence;

namespace Ubs.Monitoring.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for FX rate data access operations.
/// </summary>
public sealed class FxRateRepository : IFxRateRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<FxRateRepository> _logger;

    public FxRateRepository(AppDbContext db, ILogger<FxRateRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<FxRate?> GetLatestAsync(string baseCurrencyCode, string quoteCurrencyCode, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(baseCurrencyCode);
        ArgumentNullException.ThrowIfNull(quoteCurrencyCode);

        var normalizedBase = baseCurrencyCode.Trim().ToUpperInvariant();
        var normalizedQuote = quoteCurrencyCode.Trim().ToUpperInvariant();

        _logger.LogDebug("Retrieving latest FX rate for {Base}/{Quote}", normalizedBase, normalizedQuote);

        var fxRate = await _db.FxRates
            .AsNoTracking()
            .Where(f => f.BaseCurrencyCode == normalizedBase && f.QuoteCurrencyCode == normalizedQuote)
            .OrderByDescending(f => f.AsOfUtc)
            .FirstOrDefaultAsync(ct);

        if (fxRate is null)
        {
            _logger.LogWarning("No FX rate found for {Base}/{Quote}", normalizedBase, normalizedQuote);
        }
        else
        {
            _logger.LogDebug("Found FX rate for {Base}/{Quote}: {Rate} as of {AsOf}",
                normalizedBase, normalizedQuote, fxRate.Rate, fxRate.AsOfUtc);
        }

        return fxRate;
    }

    public async Task<FxRate?> GetByIdAsync(Guid fxRateId, CancellationToken ct)
    {
        _logger.LogDebug("Retrieving FX rate {FxRateId}", fxRateId);

        return await _db.FxRates
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == fxRateId, ct);
    }

    public async Task<bool> ExistsAsync(string baseCurrencyCode, string quoteCurrencyCode, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(baseCurrencyCode);
        ArgumentNullException.ThrowIfNull(quoteCurrencyCode);

        var normalizedBase = baseCurrencyCode.Trim().ToUpperInvariant();
        var normalizedQuote = quoteCurrencyCode.Trim().ToUpperInvariant();

        return await _db.FxRates
            .AsNoTracking()
            .AnyAsync(f => f.BaseCurrencyCode == normalizedBase && f.QuoteCurrencyCode == normalizedQuote, ct);
    }

    public async Task<FxRate?> GetByPairAndTimeWindowAsync(
        string baseCurrencyCode,
        string quoteCurrencyCode,
        DateTimeOffset asOfUtc,
        int windowMinutes,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(baseCurrencyCode);
        ArgumentNullException.ThrowIfNull(quoteCurrencyCode);

        var normalizedBase = baseCurrencyCode.Trim().ToUpperInvariant();
        var normalizedQuote = quoteCurrencyCode.Trim().ToUpperInvariant();

        var windowStart = asOfUtc.AddMinutes(-windowMinutes);
        var windowEnd = asOfUtc.AddMinutes(windowMinutes);

        _logger.LogDebug("Looking for FX rate {Base}/{Quote} within window [{Start} - {End}]",
            normalizedBase, normalizedQuote, windowStart, windowEnd);

        return await _db.FxRates
            .AsNoTracking()
            .Where(f => f.BaseCurrencyCode == normalizedBase &&
                        f.QuoteCurrencyCode == normalizedQuote &&
                        f.AsOfUtc >= windowStart &&
                        f.AsOfUtc <= windowEnd)
            .OrderByDescending(f => f.AsOfUtc)
            .FirstOrDefaultAsync(ct);
    }

    public void Add(FxRate fxRate)
    {
        ArgumentNullException.ThrowIfNull(fxRate);
        _db.FxRates.Add(fxRate);
        _logger.LogDebug("Added FX rate {Base}/{Quote} = {Rate} as of {AsOf}",
            fxRate.BaseCurrencyCode, fxRate.QuoteCurrencyCode, fxRate.Rate, fxRate.AsOfUtc);
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}

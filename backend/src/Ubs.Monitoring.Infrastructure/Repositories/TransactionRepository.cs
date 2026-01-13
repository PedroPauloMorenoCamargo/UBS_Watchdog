using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ubs.Monitoring.Application.Transactions;
using Ubs.Monitoring.Application.Transactions.Repositories;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Domain.Enums;
using Ubs.Monitoring.Infrastructure.Persistence;

namespace Ubs.Monitoring.Infrastructure.Repositories;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<TransactionRepository> _logger;

    public TransactionRepository(AppDbContext db, ILogger<TransactionRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    #region Read Operations

    public async Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken ct)
    {
        _logger.LogDebug("Retrieving transaction {TransactionId}", transactionId);

        return await _db.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transactionId, ct);
    }

    public async Task<Transaction?> GetByIdWithDetailsAsync(Guid transactionId, CancellationToken ct)
    {
        _logger.LogDebug("Retrieving transaction {TransactionId} with details", transactionId);

        return await _db.Transactions.AsNoTracking()
            .Include(t => t.Account)
            .Include(t => t.Client)
            .Include(t => t.FxRate)
            .FirstOrDefaultAsync(t => t.Id == transactionId, ct);
    }

    public async Task<IReadOnlyList<Transaction>> GetByClientIdAsync(Guid clientId, CancellationToken ct)
    {
        _logger.LogDebug("Retrieving transactions for client {ClientId}", clientId);

        var transactions = await _db.Transactions.AsNoTracking()
            .Where(t => t.ClientId == clientId)
            .OrderByDescending(t => t.OccurredAtUtc)
            .ToListAsync(ct);

        _logger.LogDebug("Found {Count} transactions for client {ClientId}", transactions.Count, clientId);

        return transactions;
    }

    public async Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken ct)
    {
        _logger.LogDebug("Retrieving transactions for account {AccountId}", accountId);

        var transactions = await _db.Transactions.AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.OccurredAtUtc)
            .ToListAsync(ct);

        _logger.LogDebug("Found {Count} transactions for account {AccountId}", transactions.Count, accountId);

        return transactions;
    }

    public async Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedAsync(
        TransactionFilterRequest filter,
        CancellationToken ct)
    {
        _logger.LogDebug("Retrieving paged transactions with filter: {@Filter}", filter);

        var query = _db.Transactions.AsNoTracking().AsQueryable();

        // Apply filters
        if (filter.ClientId.HasValue)
            query = query.Where(t => t.ClientId == filter.ClientId.Value);

        if (filter.AccountId.HasValue)
            query = query.Where(t => t.AccountId == filter.AccountId.Value);

        if (filter.Type.HasValue)
            query = query.Where(t => t.Type == filter.Type.Value);

        if (filter.TransferMethod.HasValue)
            query = query.Where(t => t.TransferMethod == filter.TransferMethod.Value);

        if (filter.DateFrom.HasValue)
            query = query.Where(t => t.OccurredAtUtc >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(t => t.OccurredAtUtc <= filter.DateTo.Value);

        if (filter.MinAmount.HasValue)
            query = query.Where(t => t.BaseAmount >= filter.MinAmount.Value);

        if (filter.MaxAmount.HasValue)
            query = query.Where(t => t.BaseAmount <= filter.MaxAmount.Value);

        if (!string.IsNullOrWhiteSpace(filter.CurrencyCode))
            query = query.Where(t => t.CurrencyCode == filter.CurrencyCode.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(filter.CpCountryCode))
            query = query.Where(t => t.CpCountryCode == filter.CpCountryCode.Trim().ToUpperInvariant());

        // Get total count before pagination
        var totalCount = await query.CountAsync(ct);

        // Apply sorting
        query = ApplySorting(query, filter.SortBy, filter.SortDescending);

        // Apply pagination
        var skip = (filter.Page - 1) * filter.PageSize;
        var items = await query.Skip(skip)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        _logger.LogDebug("Found {Count} transactions (total: {TotalCount})", items.Count, totalCount);

        return (items, totalCount);
    }

    public async Task<decimal> GetDailyTotalByClientAsync(Guid clientId, DateOnly date, CancellationToken ct)
    {
        _logger.LogDebug("Calculating daily total by client {ClientId} on {Date}", clientId, date);

        var (from, to) = UtcDayRange(date);

        // SumAsync on decimal returns 0 for empty sets in EF Core. If provider differs, wrap with DefaultIfEmpty(0m).
        var total = await _db.Transactions
            .AsNoTracking()
            .Where(t => t.ClientId == clientId &&
                        t.OccurredAtUtc >= from &&
                        t.OccurredAtUtc < to)
            .SumAsync(t => t.BaseAmount, ct);

        _logger.LogDebug("Daily total by client {ClientId} on {Date}: {Total}", clientId, date, total);

        return total;
    }

    public async Task<decimal> GetDailyTotalByAccountAsync(Guid accountId, DateOnly date, CancellationToken ct)
    {
        _logger.LogDebug("Calculating daily total by account {AccountId} on {Date}", accountId, date);

        var (from, to) = UtcDayRange(date);

        var total = await _db.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId &&
                        t.OccurredAtUtc >= from &&
                        t.OccurredAtUtc < to)
            .SumAsync(t => t.BaseAmount, ct);

        _logger.LogDebug("Daily total by account {AccountId} on {Date}: {Total}", accountId, date, total);

        return total;
    }

    public async Task<int> CountDailyTransfersUnderBaseAmountByClientAsync(
        Guid clientId,
        DateOnly date,
        decimal maxBaseAmount,
        CancellationToken ct)
    {
        _logger.LogDebug(
            "Counting daily transfers by client {ClientId} on {Date} with BaseAmount <= {MaxBaseAmount}",
            clientId, date, maxBaseAmount);

        var (from, to) = UtcDayRange(date);

        var count = await _db.Transactions
            .AsNoTracking()
            .Where(t =>
                t.ClientId == clientId &&
                t.Type == TransactionType.Transfer &&
                t.BaseAmount <= maxBaseAmount &&
                t.OccurredAtUtc >= from &&
                t.OccurredAtUtc < to)
            .CountAsync(ct);

        _logger.LogDebug(
            "Daily transfer count by client {ClientId} on {Date} (<= {MaxBaseAmount}): {Count}",
            clientId, date, maxBaseAmount, count);

        return count;
    }

    public async Task<int> CountDailyTransfersUnderBaseAmountByAccountAsync(
        Guid accountId,
        DateOnly date,
        decimal maxBaseAmount,
        CancellationToken ct)
    {
        _logger.LogDebug(
            "Counting daily transfers by account {AccountId} on {Date} with BaseAmount <= {MaxBaseAmount}",
            accountId, date, maxBaseAmount);

        var (from, to) = UtcDayRange(date);

        var count = await _db.Transactions
            .AsNoTracking()
            .Where(t =>
                t.AccountId == accountId &&
                t.Type == TransactionType.Transfer &&
                t.BaseAmount <= maxBaseAmount &&
                t.OccurredAtUtc >= from &&
                t.OccurredAtUtc < to)
            .CountAsync(ct);

        _logger.LogDebug(
            "Daily transfer count by account {AccountId} on {Date} (<= {MaxBaseAmount}): {Count}",
            accountId, date, maxBaseAmount, count);

        return count;
    }

    public async Task<bool> ExistsAsync(Guid transactionId, CancellationToken ct)
    {
        return await _db.Transactions
            .AsNoTracking()
            .AnyAsync(t => t.Id == transactionId, ct);
    }

    #endregion

    #region Write Operations

    public void Add(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        _logger.LogDebug("Adding transaction {TransactionId} for client {ClientId}",
            transaction.Id, transaction.ClientId);

        _db.Transactions.Add(transaction);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        _logger.LogDebug("Saving transaction changes to database");

        await _db.SaveChangesAsync(ct);

        _logger.LogDebug("Transaction changes saved successfully");
    }

    #endregion

    #region Private Methods

    private static (DateTimeOffset from, DateTimeOffset to) UtcDayRange(DateOnly day)
    {
        // UTC midnight start; end is exclusive
        var from = new DateTimeOffset(day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        return (from, from.AddDays(1));
    }

    private static IQueryable<Transaction> ApplySorting(
        IQueryable<Transaction> query,
        string? sortBy,
        bool sortDescending)
    {
        // Default sort by OccurredAtUtc descending
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return sortDescending
                ? query.OrderByDescending(t => t.OccurredAtUtc)
                : query.OrderBy(t => t.OccurredAtUtc);
        }

        return sortBy.ToLowerInvariant() switch
        {
            "occurredatutc" or "occurred" or "date" => sortDescending
                ? query.OrderByDescending(t => t.OccurredAtUtc)
                : query.OrderBy(t => t.OccurredAtUtc),

            "amount" => sortDescending
                ? query.OrderByDescending(t => t.Amount)
                : query.OrderBy(t => t.Amount),

            "baseamount" => sortDescending
                ? query.OrderByDescending(t => t.BaseAmount)
                : query.OrderBy(t => t.BaseAmount),

            "type" => sortDescending
                ? query.OrderByDescending(t => t.Type)
                : query.OrderBy(t => t.Type),

            "createdatutc" or "created" => sortDescending
                ? query.OrderByDescending(t => t.CreatedAtUtc)
                : query.OrderBy(t => t.CreatedAtUtc),

            _ => sortDescending
                ? query.OrderByDescending(t => t.OccurredAtUtc)
                : query.OrderBy(t => t.OccurredAtUtc)
        };
    }

    #endregion
}

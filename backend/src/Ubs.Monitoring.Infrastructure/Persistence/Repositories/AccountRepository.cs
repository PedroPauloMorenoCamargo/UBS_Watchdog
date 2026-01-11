using Microsoft.EntityFrameworkCore;
using Ubs.Monitoring.Application.Accounts;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for account data access operations.
/// </summary>
public sealed class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _db;

    public AccountRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Retrieves an account by its unique identifier.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matching account if found; otherwise, null.</returns>
    public Task<Account?> GetByIdAsync(Guid accountId, CancellationToken ct)
        => _db.Accounts
              .AsNoTracking()
              .FirstOrDefaultAsync(a => a.Id == accountId, ct);

    /// <summary>
    /// Retrieves an account with detailed information including identifiers.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The account with related data if found; otherwise, null.</returns>
    public Task<Account?> GetByIdWithDetailsAsync(Guid accountId, CancellationToken ct)
        => _db.Accounts
              .AsNoTracking()
              .Include(a => a.Identifiers)
              .Include(a => a.Client)
              .FirstOrDefaultAsync(a => a.Id == accountId, ct);

    /// <summary>
    /// Retrieves all accounts for a specific client.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of accounts belonging to the client.</returns>
    public async Task<IReadOnlyList<Account>> GetByClientIdAsync(Guid clientId, CancellationToken ct)
        => await _db.Accounts
              .AsNoTracking()
              .Include(a => a.Identifiers)
              .Where(a => a.ClientId == clientId)
              .OrderByDescending(a => a.CreatedAtUtc)
              .ToListAsync(ct);

    /// <summary>
    /// Checks if an account identifier already exists.
    /// </summary>
    /// <param name="accountIdentifier">The account identifier to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the identifier exists; otherwise, false.</returns>
    public Task<bool> AccountIdentifierExistsAsync(string accountIdentifier, CancellationToken ct)
        => _db.Accounts.AnyAsync(a => a.AccountIdentifier == accountIdentifier, ct);

    /// <summary>
    /// Checks if an account with the specified ID exists.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the account exists; otherwise, false.</returns>
    public Task<bool> ExistsAsync(Guid accountId, CancellationToken ct)
        => _db.Accounts.AnyAsync(a => a.Id == accountId, ct);

    /// <summary>
    /// Adds a new account to the database context.
    /// Call SaveChangesAsync to persist changes to the database.
    /// </summary>
    /// <param name="account">The account entity to add.</param>
    public void Add(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);
        _db.Accounts.Add(account);
    }

    /// <summary>
    /// Retrieves an account with identifiers for update operations (tracked).
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The tracked account entity with identifiers if found; otherwise, null.</returns>
    public Task<Account?> GetForUpdateWithIdentifiersAsync(Guid accountId, CancellationToken ct)
        => _db.Accounts
              .Include(a => a.Identifiers)
              .FirstOrDefaultAsync(a => a.Id == accountId, ct);

    /// <summary>
    /// Retrieves an account identifier by its unique identifier.
    /// </summary>
    /// <param name="identifierId">The unique identifier of the account identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The account identifier if found; otherwise, null.</returns>
    public Task<AccountIdentifier?> GetIdentifierByIdAsync(Guid identifierId, CancellationToken ct)
        => _db.AccountIdentifiers
              .FirstOrDefaultAsync(i => i.Id == identifierId, ct);

    /// <summary>
    /// Removes an account identifier from the database context.
    /// Call SaveChangesAsync to persist changes to the database.
    /// </summary>
    /// <param name="identifier">The account identifier entity to remove.</param>
    public void RemoveIdentifier(AccountIdentifier identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        _db.AccountIdentifiers.Remove(identifier);
    }

    /// <summary>
    /// Adds a new account identifier to the database context.
    /// Call SaveChangesAsync to persist changes to the database.
    /// </summary>
    /// <param name="identifier">The account identifier entity to add.</param>
    public void AddIdentifier(AccountIdentifier identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        _db.AccountIdentifiers.Add(identifier);
    }

    /// <summary>
    /// Checks if an identifier with the specified type and value already exists for an account.
    /// </summary>
    /// <param name="accountId">The account ID to check.</param>
    /// <param name="identifierType">The type of identifier.</param>
    /// <param name="identifierValue">The value of the identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the identifier exists; otherwise, false.</returns>
    public Task<bool> IdentifierExistsAsync(Guid accountId, IdentifierType identifierType, string identifierValue, CancellationToken ct)
        => _db.AccountIdentifiers.AnyAsync(
            i => i.AccountId == accountId && i.IdentifierType == identifierType && i.IdentifierValue == identifierValue,
            ct);

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}

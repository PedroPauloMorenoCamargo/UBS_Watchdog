using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Accounts;

/// <summary>
/// Repository interface for account data access operations.
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Retrieves an account by its unique identifier.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The account if found; otherwise, null.</returns>
    Task<Account?> GetByIdAsync(Guid accountId, CancellationToken ct);

    /// <summary>
    /// Retrieves an account with detailed information including identifiers.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The account with related data if found; otherwise, null.</returns>
    Task<Account?> GetByIdWithDetailsAsync(Guid accountId, CancellationToken ct);

    /// <summary>
    /// Retrieves all accounts for a specific client.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of accounts belonging to the client.</returns>
    Task<IReadOnlyList<Account>> GetByClientIdAsync(Guid clientId, CancellationToken ct);

    /// <summary>
    /// Checks if an account identifier already exists.
    /// </summary>
    /// <param name="accountIdentifier">The account identifier to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the identifier exists; otherwise, false.</returns>
    Task<bool> AccountIdentifierExistsAsync(string accountIdentifier, CancellationToken ct);

    /// <summary>
    /// Retrieves an account by its account identifier string.
    /// </summary>
    /// <param name="accountIdentifier">The account identifier (e.g., "BR-001-12345-6").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The account if found; otherwise, null.</returns>
    Task<Account?> GetByAccountIdentifierAsync(string accountIdentifier, CancellationToken ct);

    /// <summary>
    /// Checks if an account with the specified ID exists.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the account exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(Guid accountId, CancellationToken ct);

    /// <summary>
    /// Adds a new account to the database context.
    /// Call SaveChangesAsync to persist changes to the database.
    /// </summary>
    /// <param name="account">The account entity to add.</param>
    void Add(Account account);

    /// <summary>
    /// Retrieves an account with identifiers for update operations (tracked).
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The tracked account entity with identifiers if found; otherwise, null.</returns>
    Task<Account?> GetForUpdateWithIdentifiersAsync(Guid accountId, CancellationToken ct);

    /// <summary>
    /// Retrieves an account identifier by its unique identifier.
    /// </summary>
    /// <param name="identifierId">The unique identifier of the account identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The account identifier if found; otherwise, null.</returns>
    Task<AccountIdentifier?> GetIdentifierByIdAsync(Guid identifierId, CancellationToken ct);

    /// <summary>
    /// Removes an account identifier from the database context.
    /// Call SaveChangesAsync to persist changes to the database.
    /// </summary>
    /// <param name="identifier">The account identifier entity to remove.</param>
    void RemoveIdentifier(AccountIdentifier identifier);

    /// <summary>
    /// Adds a new account identifier to the database context.
    /// Call SaveChangesAsync to persist changes to the database.
    /// </summary>
    /// <param name="identifier">The account identifier entity to add.</param>
    void AddIdentifier(AccountIdentifier identifier);

    /// <summary>
    /// Checks if an identifier with the specified type and value already exists for an account.
    /// </summary>
    /// <param name="accountId">The account ID to check.</param>
    /// <param name="identifierType">The type of identifier.</param>
    /// <param name="identifierValue">The value of the identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the identifier exists; otherwise, false.</returns>
    Task<bool> IdentifierExistsAsync(Guid accountId, IdentifierType identifierType, string identifierValue, CancellationToken ct);

    /// <summary>
    /// Checks if an identifier with the specified type and value exists globally (across all accounts).
    /// Used for validating counterparty identifiers in transfer transactions.
    /// </summary>
    /// <param name="identifierType">The type of identifier.</param>
    /// <param name="identifierValue">The value of the identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the identifier exists; otherwise, false.</returns>
    Task<bool> IdentifierExistsGloballyAsync(IdentifierType identifierType, string identifierValue, CancellationToken ct);

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task SaveChangesAsync(CancellationToken ct);
}

using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.Transactions.Repositories;

/// <summary>
/// Repository interface for transaction data access operations.
/// </summary>
public interface ITransactionRepository
{
    #region Read Operations

    /// <summary>
    /// Retrieves a transaction by its unique identifier.
    /// </summary>
    /// <param name="transactionId">The unique identifier of the transaction.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The transaction if found; otherwise, null.</returns>
    Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken ct);

    /// <summary>
    /// Retrieves a transaction with detailed information including Account and Client.
    /// </summary>
    /// <param name="transactionId">The unique identifier of the transaction.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The transaction with related data if found; otherwise, null.</returns>
    Task<Transaction?> GetByIdWithDetailsAsync(Guid transactionId, CancellationToken ct);

    /// <summary>
    /// Retrieves all transactions for a specific client.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of transactions belonging to the client, ordered by OccurredAtUtc descending.</returns>
    Task<IReadOnlyList<Transaction>> GetByClientIdAsync(Guid clientId, CancellationToken ct);

    /// <summary>
    /// Retrieves all transactions for a specific account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of transactions for the account, ordered by OccurredAtUtc descending.</returns>
    Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken ct);

    /// <summary>
    /// Retrieves a paginated and filtered list of transactions.
    /// </summary>
    /// <param name="filter">The filter and pagination parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated result with transactions and total count.</returns>
    Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedAsync( TransactionFilterRequest filter, CancellationToken ct);

    // <summary>
    /// Gets the total base amount of transactions for a client on a specific date (UTC).
    /// Intended for compliance rule evaluation.
    /// </summary>
    Task<decimal> GetDailyTotalByClientAsync(Guid clientId, DateOnly date, CancellationToken ct);

    /// <summary>
    /// Gets the total base amount of transactions for an account on a specific date (UTC).
    /// Intended for compliance rule evaluation.
    /// </summary>
    Task<decimal> GetDailyTotalByAccountAsync(Guid accountId, DateOnly date, CancellationToken ct);

    /// <summary>
    /// Counts how many TRANSFER transactions for a client on a specific date (UTC) have BaseAmount <= maxBaseAmount.
    /// Intended for structuring detection rules.
    /// </summary>
    Task<int> CountDailyTransfersUnderBaseAmountByClientAsync(Guid clientId, DateOnly date, decimal maxBaseAmount, CancellationToken ct);

    /// <summary>
    /// Counts how many TRANSFER transactions for an account on a specific date (UTC) have BaseAmount <= maxBaseAmount.
    /// Intended for structuring detection rules.
    /// </summary>
    Task<int> CountDailyTransfersUnderBaseAmountByAccountAsync( Guid accountId, DateOnly date, decimal maxBaseAmount, CancellationToken ct);


    /// <summary>
    /// Checks if a transaction with the specified ID exists.
    /// </summary>
    /// <param name="transactionId">The unique identifier of the transaction.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the transaction exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(Guid transactionId, CancellationToken ct);

    #endregion

    #region Write Operations

    /// <summary>
    /// Adds a new transaction to the database context.
    /// Call SaveChangesAsync to persist changes to the database.
    /// </summary>
    /// <param name="transaction">The transaction entity to add.</param>
    void Add(Transaction transaction);

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task SaveChangesAsync(CancellationToken ct);

    #endregion
}

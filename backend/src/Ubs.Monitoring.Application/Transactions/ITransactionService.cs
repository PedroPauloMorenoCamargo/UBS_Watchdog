namespace Ubs.Monitoring.Application.Transactions;

/// <summary>
/// Service interface for transaction business operations.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Creates a new transaction.
    /// </summary>
    /// <param name="request">The transaction creation request data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the created transaction data and error message.
    /// If successful, Result contains the transaction data and ErrorMessage is null.
    /// If failed, Result is null and ErrorMessage contains the validation error.
    /// </returns>
    Task<(TransactionResponseDto? Result, string? ErrorMessage)> CreateTransactionAsync(
        CreateTransactionRequest request,
        CancellationToken ct);

    /// <summary>
    /// Retrieves a transaction by its unique identifier.
    /// </summary>
    /// <param name="transactionId">The unique identifier of the transaction.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The transaction details if found; otherwise, null.</returns>
    Task<TransactionDetailDto?> GetTransactionByIdAsync(Guid transactionId, CancellationToken ct);

    /// <summary>
    /// Retrieves a paginated and filtered list of transactions.
    /// </summary>
    /// <param name="filter">The filter and pagination parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A paginated result containing transactions.</returns>
    Task<PagedTransactionsResponseDto> GetTransactionsAsync(
        TransactionFilterRequest filter,
        CancellationToken ct);

    /// <summary>
    /// Imports multiple transactions from a CSV or Excel file.
    /// </summary>
    /// <param name="fileStream">The file stream containing transaction data.</param>
    /// <param name="fileName">The file name (used to determine format).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the import result and error message.
    /// Result contains the import summary with success/error counts.
    /// ErrorMessage contains any general parsing or file error.
    /// </returns>
    Task<(TransactionImportResultDto? Result, string? ErrorMessage)> ImportTransactionsFromFileAsync(
        Stream fileStream,
        string fileName,
        CancellationToken ct);
}

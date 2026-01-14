namespace Ubs.Monitoring.Application.Accounts;

/// <summary>
/// Service interface for account business operations.
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Creates a new account for a client.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="request">The account creation request data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the created account data and error message.
    /// If successful, Result contains the account data and ErrorMessage is null.
    /// If failed, Result is null and ErrorMessage contains the validation error.
    /// </returns>
    Task<(AccountResponseDto? Result, string? ErrorMessage)> CreateAccountAsync(
        Guid clientId,
        CreateAccountRequest request,
        CancellationToken ct);

    /// <summary>
    /// Retrieves all accounts for a specific client.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the list of accounts and error message.
    /// If the client exists, Result contains the accounts (may be empty) and ErrorMessage is null.
    /// If the client does not exist, Result is null and ErrorMessage contains the error.
    /// </returns>
    Task<(IReadOnlyList<AccountResponseDto>? Result, string? ErrorMessage)> GetAccountsByClientIdAsync(
        Guid clientId,
        CancellationToken ct);

    /// <summary>
    /// Retrieves detailed information about a specific account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The account details if found; otherwise, null.</returns>
    Task<AccountDetailDto?> GetAccountByIdAsync(Guid accountId, CancellationToken ct);

    /// <summary>
    /// Imports multiple accounts for a client from a CSV or Excel file.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="fileStream">The file stream containing account data.</param>
    /// <param name="fileName">The file name (used to determine format).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the import result and error message.
    /// If the client exists, Result contains the import summary and ErrorMessage is null.
    /// If the client does not exist, Result is null and ErrorMessage contains the error.
    /// </returns>
    Task<(AccountImportResultDto? Result, string? ErrorMessage)> ImportAccountsFromFileAsync(
        Guid clientId,
        Stream fileStream,
        string fileName,
        CancellationToken ct);
}

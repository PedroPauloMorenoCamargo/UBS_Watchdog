namespace Ubs.Monitoring.Application.AccountIdentifiers;

/// <summary>
/// Service interface for account identifier business operations.
/// </summary>
public interface IAccountIdentifierService
{
    /// <summary>
    /// Retrieves all identifiers for a specific account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the list of identifiers and error message.
    /// If the account exists, Result contains the identifiers and ErrorMessage is null.
    /// If the account does not exist, Result is null and ErrorMessage contains the error.
    /// </returns>
    Task<(IReadOnlyList<AccountIdentifierDto>? Result, string? ErrorMessage)> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken ct);

    /// <summary>
    /// Creates a new identifier for an account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="request">The identifier creation request data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the created identifier data and error message.
    /// If successful, Result contains the identifier data and ErrorMessage is null.
    /// If failed, Result is null and ErrorMessage contains the validation error.
    /// </returns>
    Task<(AccountIdentifierDto? Result, string? ErrorMessage)> CreateIdentifierAsync(
        Guid accountId,
        CreateAccountIdentifierRequest request,
        CancellationToken ct);

    /// <summary>
    /// Removes an identifier from an account.
    /// </summary>
    /// <param name="identifierId">The unique identifier of the account identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing success flag and error message.
    /// If successful, Success is true and ErrorMessage is null.
    /// If failed, Success is false and ErrorMessage contains the error.
    /// </returns>
    Task<(bool Success, string? ErrorMessage)> RemoveIdentifierAsync(
        Guid identifierId,
        CancellationToken ct);
}

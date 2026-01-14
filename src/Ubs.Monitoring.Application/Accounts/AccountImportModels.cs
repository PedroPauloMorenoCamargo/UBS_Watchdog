using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Accounts;

/// <summary>
/// Represents a row from CSV/Excel import file for accounts.
/// </summary>
public sealed class AccountImportRow
{
    public string AccountIdentifier { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Converts the import row to a CreateAccountRequest.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when AccountType is invalid.</exception>
    public CreateAccountRequest ToRequest()
    {
        // Parse AccountType enum (required)
        if (!Enum.TryParse<AccountType>(AccountType, ignoreCase: true, out var accountTypeEnum))
            throw new InvalidOperationException(
                $"Invalid AccountType: {AccountType}. Must be 'Checking', 'Savings', 'Investment', or 'Other'.");

        return new CreateAccountRequest(
            AccountIdentifier: AccountIdentifier,
            CountryCode: CountryCode,
            AccountType: accountTypeEnum,
            CurrencyCode: CurrencyCode
        );
    }
}

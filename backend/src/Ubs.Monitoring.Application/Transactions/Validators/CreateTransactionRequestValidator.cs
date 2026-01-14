using FluentValidation;
using Ubs.Monitoring.Application.Accounts;
using Ubs.Monitoring.Application.Countries;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Transactions.Validators;

/// <summary>
/// Validator for transaction creation requests.
/// </summary>
public sealed class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    private readonly IAccountRepository _accounts;
    private readonly ICountryRepository _countries;

    public CreateTransactionRequestValidator(IAccountRepository accounts, ICountryRepository countries)
    {
        _accounts = accounts;
        _countries = countries;

        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required.")
            .MustAsync(async (accountId, ct) => await _accounts.ExistsAsync(accountId, ct))
            .WithMessage("Account with ID '{PropertyValue}' does not exist.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Transaction type must be a valid value (Deposit, Withdrawal, or Transfer).");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Transaction amount must be greater than zero.");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .WithMessage("Currency code is required.")
            .Length(3)
            .WithMessage("Currency code must be exactly 3 characters.")
            .Matches(@"^[A-Za-z]{3}$")
            .WithMessage("Currency code must contain only letters. Example: BRL, USD, EUR.");

        RuleFor(x => x.OccurredAtUtc)
            .NotEmpty()
            .WithMessage("Transaction date/time is required.")
            .LessThanOrEqualTo(DateTimeOffset.UtcNow.AddMinutes(5))
            .WithMessage("Transaction date/time cannot be in the future.");

        // Transfer-specific validations
        When(x => x.Type == TransactionType.Transfer, () =>
        {
            RuleFor(x => x.TransferMethod)
                .NotNull()
                .WithMessage("Transfer method is required for Transfer transactions.")
                .IsInEnum()
                .WithMessage("Transfer method must be a valid value (PIX, TED, or WIRE).");

            RuleFor(x => x.CpCountryCode)
                .NotEmpty()
                .WithMessage("Counterparty country code is required for Transfer transactions.")
                .Length(2)
                .WithMessage("Counterparty country code must be exactly 2 characters.")
                .Matches(@"^[A-Za-z]{2}$")
                .WithMessage("Counterparty country code must contain only letters. Example: BR, US.")
                .MustAsync(async (countryCode, ct) =>
                {
                    if (string.IsNullOrWhiteSpace(countryCode)) return true;
                    var normalizedCode = countryCode.Trim().ToUpperInvariant();
                    return await _countries.ExistsAsync(normalizedCode, ct);
                })
                .WithMessage("Invalid country code '{PropertyValue}'. This country does not exist in the countries table. Please use a valid ISO 3166-1 alpha-2 code (e.g., BR, US, GB, DE).");

            RuleFor(x => x.CpIdentifierType)
                .NotNull()
                .WithMessage("Counterparty identifier type is required for Transfer transactions.")
                .IsInEnum()
                .WithMessage("Counterparty identifier type must be a valid value.");

            RuleFor(x => x.CpIdentifier)
                .NotEmpty()
                .WithMessage("Counterparty identifier is required for Transfer transactions.")
                .MaximumLength(200)
                .WithMessage("Counterparty identifier cannot exceed 200 characters.");

            // Validate cpIdentifier exists in account_identifiers table when cpCountryCode is BR
            RuleFor(x => x)
                .MustAsync(async (request, ct) =>
                {
                    // Only validate for Brazilian transfers
                    if (string.IsNullOrWhiteSpace(request.CpCountryCode) ||
                        !request.CpCountryCode.Trim().Equals("BR", StringComparison.OrdinalIgnoreCase))
                    {
                        return true; // Skip validation for non-BR countries
                    }

                    // Must have identifier type and value to validate
                    if (request.CpIdentifierType is null || string.IsNullOrWhiteSpace(request.CpIdentifier))
                    {
                        return true; // Other validations will catch this
                    }

                    // Check if the identifier exists globally
                    return await _accounts.IdentifierExistsGloballyAsync(
                        request.CpIdentifierType.Value,
                        request.CpIdentifier.Trim(),
                        ct);
                })
                .WithMessage(request =>
                    $"Counterparty identifier '{request.CpIdentifier}' of type '{request.CpIdentifierType}' was not found in the system. " +
                    $"For Brazilian transfers (BR), the counterparty must have a registered account with this identifier.")
                .When(x => x.Type == TransactionType.Transfer);
        });

        // Optional field validations (when provided)
        When(x => !string.IsNullOrWhiteSpace(x.CpName), () =>
        {
            RuleFor(x => x.CpName)
                .MaximumLength(200)
                .WithMessage("Counterparty name cannot exceed 200 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.CpBank), () =>
        {
            RuleFor(x => x.CpBank)
                .MaximumLength(200)
                .WithMessage("Counterparty bank cannot exceed 200 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.CpBranch), () =>
        {
            RuleFor(x => x.CpBranch)
                .MaximumLength(50)
                .WithMessage("Counterparty branch cannot exceed 50 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.CpAccount), () =>
        {
            RuleFor(x => x.CpAccount)
                .MaximumLength(80)
                .WithMessage("Counterparty account cannot exceed 80 characters.");
        });
    }
}

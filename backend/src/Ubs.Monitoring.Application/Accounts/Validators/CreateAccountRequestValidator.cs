using FluentValidation;
using Ubs.Monitoring.Application.Countries;

namespace Ubs.Monitoring.Application.Accounts.Validators;

/// <summary>
/// Validator for account creation requests.
/// </summary>
public sealed class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    private readonly ICountryRepository _countries;
    private readonly IAccountRepository _accounts;

    public CreateAccountRequestValidator(
        ICountryRepository countries,
        IAccountRepository accounts)
    {
        _countries = countries;
        _accounts = accounts;

        RuleFor(x => x.AccountIdentifier)
            .NotEmpty()
            .WithMessage("Account identifier is required.")
            .MaximumLength(80)
            .WithMessage("Account identifier cannot exceed 80 characters.")
            .Must(identifier => !string.IsNullOrWhiteSpace(identifier))
            .WithMessage("Account identifier cannot contain only whitespace characters.")
            .MustAsync(async (identifier, ct) =>
            {
                var trimmed = identifier.Trim();
                return !await _accounts.AccountIdentifierExistsAsync(trimmed, ct);
            })
            .WithMessage("Account identifier '{PropertyValue}' already exists in the system.");

        RuleFor(x => x.CountryCode)
            .NotEmpty()
            .WithMessage("Country code is required.")
            .Length(2)
            .WithMessage("Country code must be exactly 2 characters.")
            .Matches(@"^[A-Za-z]{2}$")
            .WithMessage("Country code must contain only letters. Example: BR, US, DE.")
            .MustAsync(async (countryCode, ct) =>
            {
                var normalizedCode = countryCode.Trim().ToUpperInvariant();
                return await _countries.ExistsAsync(normalizedCode, ct);
            })
            .WithMessage("Country code '{PropertyValue}' does not exist in the system.");

        RuleFor(x => x.AccountType)
            .IsInEnum()
            .WithMessage("Account type must be a valid value (Checking, Savings, Investment, or Other).");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .WithMessage("Currency code is required.")
            .Length(3)
            .WithMessage("Currency code must be exactly 3 characters.")
            .Matches(@"^[A-Za-z]{3}$")
            .WithMessage("Currency code must contain only letters. Example: BRL, USD, EUR.");
    }
}

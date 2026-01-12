using FluentValidation;
using Ubs.Monitoring.Application.Countries;

namespace Ubs.Monitoring.Application.AccountIdentifiers.Validators;

/// <summary>
/// Validator for account identifier creation requests.
/// Ensures data integrity and compliance with business rules before identifier entity creation.
/// </summary>
public sealed class CreateAccountIdentifierRequestValidator : AbstractValidator<CreateAccountIdentifierRequest>
{
    public CreateAccountIdentifierRequestValidator(ICountryRepository countries)
    {
        RuleFor(x => x.IdentifierType)
            .IsInEnum()
            .WithMessage("Identifier type must be a valid value.");

        RuleFor(x => x.IdentifierValue)
            .NotEmpty()
            .WithMessage("Identifier value is required.")
            .MaximumLength(200)
            .WithMessage("Identifier value cannot exceed 200 characters.")
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Identifier value cannot contain only whitespace characters.");

        RuleFor(x => x.IssuedCountryCode)
            .Length(2)
            .When(x => !string.IsNullOrEmpty(x.IssuedCountryCode))
            .WithMessage("Issued country code must be exactly 2 characters.")
            .Matches(@"^[A-Za-z]{2}$")
            .When(x => !string.IsNullOrEmpty(x.IssuedCountryCode))
            .WithMessage("Issued country code must contain only letters. Example: BR, US, DE.")
            .MustAsync(async (countryCode, ct) =>
            {
                if (string.IsNullOrEmpty(countryCode))
                    return true;

                var normalizedCode = countryCode.Trim().ToUpperInvariant();
                return await countries.ExistsAsync(normalizedCode, ct);
            })
            .WithMessage("Invalid country code '{PropertyValue}'. This country does not exist in the countries table. Please use a valid ISO 3166-1 alpha-2 code (e.g., BR, US, GB, DE).");
    }
}

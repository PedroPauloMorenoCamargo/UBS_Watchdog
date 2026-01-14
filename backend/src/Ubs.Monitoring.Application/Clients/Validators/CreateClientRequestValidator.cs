using FluentValidation;
using Ubs.Monitoring.Application.Countries;

namespace Ubs.Monitoring.Application.Clients.Validators;

/// <summary>
/// Validator for client creation requests.
/// Ensures data integrity and compliance with business rules before client entity creation.
/// Performs both structural validation (format) and data validation (country existence).
/// </summary>
public sealed class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
{
    private readonly ICountryRepository _countries;

    public CreateClientRequestValidator(ICountryRepository countries)
    {
        _countries = countries;
        RuleFor(x => x.LegalType)
            .IsInEnum()
            .WithMessage("Legal type must be a valid value (Individual or Corporate).");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Client name is required and cannot be empty.")
            .MaximumLength(200)
            .WithMessage("Client name cannot exceed 200 characters.")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Client name cannot contain only whitespace characters.");

        RuleFor(x => x.ContactNumber)
            .NotEmpty()
            .WithMessage("Contact number is required and cannot be empty.")
            .Must(contact => !string.IsNullOrWhiteSpace(contact))
            .WithMessage("Contact number cannot contain only whitespace characters.")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Contact number must be in valid international format.");

        RuleFor(x => x.AddressJson)
            .NotNull()
            .WithMessage("Address information is required for compliance and KYC purposes.");

        RuleFor(x => x.CountryCode)
            .NotEmpty()
            .WithMessage("Country code is required for regulatory compliance.")
            .Length(2)
            .WithMessage("Country code must be exactly 2 characters.")
            .Matches(@"^[A-Za-z]{2}$")
            .WithMessage("Country code must contain only letters. Example: BR, US, DE.")
            .MustAsync(async (countryCode, cancellationToken) =>
            {
                var normalizedCode = countryCode.Trim().ToUpperInvariant();
                return await _countries.ExistsAsync(normalizedCode, cancellationToken);
            })
            .WithMessage("Country code '{PropertyValue}' does not exist in the system. Please use a valid country code.");

        When(x => x.InitialRiskLevel.HasValue, () =>
        {
            RuleFor(x => x.InitialRiskLevel!.Value)
                .IsInEnum()
                .WithMessage("Initial risk level must be a valid value (Low, Medium, or High).");
        });
    }
}

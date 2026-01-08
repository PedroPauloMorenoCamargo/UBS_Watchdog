using FluentValidation;

namespace Ubs.Monitoring.Application.Clients.Validators;

/// <summary>
/// Validator for client contact information update requests.
/// Ensures contact data integrity and proper formatting for compliance tracking.
/// </summary>
public sealed class UpdateClientContactRequestValidator : AbstractValidator<UpdateClientContactRequest>
{
    public UpdateClientContactRequestValidator()
    {
        RuleFor(x => x.ContactNumber)
            .NotEmpty()
            .WithMessage("Contact number is required and cannot be empty.")
            .Must(contact => !string.IsNullOrWhiteSpace(contact))
            .WithMessage("Contact number cannot contain only whitespace characters.")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Contact number must be in valid international format (Ex: +5511912345678).");

        // AddressJson is optional - no validation needed (null is acceptable)
    }
}

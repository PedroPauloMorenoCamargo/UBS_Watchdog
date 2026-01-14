using FluentValidation;

namespace Ubs.Monitoring.Application.Clients.Validators;

/// <summary>
/// Validator for client risk level update requests.
/// Ensures risk classification changes meet compliance requirements.
/// </summary>
public sealed class UpdateClientRiskRequestValidator : AbstractValidator<UpdateClientRiskRequest>
{
    public UpdateClientRiskRequestValidator()
    {
        RuleFor(x => x.NewRiskLevel)
            .IsInEnum()
            .WithMessage("Risk level must be a valid value (Low, Medium, or High).");
    }
}

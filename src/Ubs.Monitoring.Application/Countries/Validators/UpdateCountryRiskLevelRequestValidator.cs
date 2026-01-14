using FluentValidation;

namespace Ubs.Monitoring.Application.Countries.Validators;

/// <summary>
/// Validator for country risk level update requests.
/// Ensures risk level changes meet compliance requirements.
/// </summary>
public sealed class UpdateCountryRiskLevelRequestValidator : AbstractValidator<UpdateCountryRiskLevelRequest>
{
    public UpdateCountryRiskLevelRequestValidator()
    {
        RuleFor(x => x.NewRiskLevel)
            .IsInEnum()
            .WithMessage("Risk level must be a valid value (Low, Medium, or High).");
    }
}

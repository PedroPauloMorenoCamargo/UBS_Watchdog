using FluentValidation;
using Ubs.Monitoring.Api.Contracts;

namespace Ubs.Monitoring.Api.Validation;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    private const int EmailMaxLen = 255;
    private const int PasswordMinLen = 8;
    private const int PasswordMaxLen = 255;

    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop).NotEmpty().WithMessage("Email is required.")
            .Must(e => !string.IsNullOrWhiteSpace(e)).WithMessage("Email must not be empty or whitespace.")
            .Must(e => e == e.Trim()).WithMessage("Email must not contain leading or trailing spaces.")
            .MaximumLength(EmailMaxLen).WithMessage($"Email must be at most {EmailMaxLen} characters.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop).NotEmpty().WithMessage("Password is required.")
            .Must(p => !string.IsNullOrWhiteSpace(p)).WithMessage("Password must not be empty or whitespace.")
            .MinimumLength(PasswordMinLen).WithMessage($"Password must be at least {PasswordMinLen} characters.")
            .MaximumLength(PasswordMaxLen).WithMessage($"Password must be at most {PasswordMaxLen} characters.");
    }
}

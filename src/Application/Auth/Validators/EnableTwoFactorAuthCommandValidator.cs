using FluentValidation;
using SDI_Api.Application.Auth.Commands;

namespace SDI_Api.Application.Auth.Validators;

public class EnableTwoFactorAuthCommandValidator : AbstractValidator<EnableTwoFactorAuthCommand>
{
    public EnableTwoFactorAuthCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.VerificationCode)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6, 7).WithMessage("Verification code must be 6 or 7 digits long.")
            .Matches("^[0-9]*$").WithMessage("Verification code must only contain digits.");
    }
}

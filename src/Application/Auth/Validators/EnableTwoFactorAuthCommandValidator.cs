using FluentValidation;
using SDI_Api.Application.Auth.Commands;

namespace SDI_Api.Application.Auth.Validators;

public class EnableTwoFactorAuthCommandValidator : AbstractValidator<EnableTwoFactorAuthCommand>
{
    public EnableTwoFactorAuthCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

using SDI_Api.Application.Auth.Commands;

namespace SDI_Api.Application.Auth.Validators;

public class ForgotPasswordValidateRecoveryCodeCommandValidator : AbstractValidator<ValidateRecoveryCodeCommand>
{
    public ForgotPasswordValidateRecoveryCodeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.RecoveryCode)
            .NotEmpty().WithMessage("Recovery code is required.");
    }
    
}

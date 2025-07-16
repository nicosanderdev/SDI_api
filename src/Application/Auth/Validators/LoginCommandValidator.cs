using SDI_Api.Application.Auth.Commands;

namespace SDI_Api.Application.Auth.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty()
            .WithMessage("Username or Email is required.");
        
        RuleFor(x => x)
            .Must(command => 
                !string.IsNullOrEmpty(command.Password) || !string.IsNullOrEmpty(command.TwoFactorCode)
            )
            .WithMessage("A Password or a Two-Factor Code is required.");
    }
}

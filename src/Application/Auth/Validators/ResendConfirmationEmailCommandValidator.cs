using FluentValidation;
using SDI_Api.Application.Auth.Commands;

namespace SDI_Api.Application.Auth.Validators;

public class ResendConfirmationEmailCommandValidator : AbstractValidator<ResendConfirmationEmailCommand>
{
    public ResendConfirmationEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Email is required.");
    }
}

using FluentValidation;
using SDI_Api.Application.Auth.Queries;

namespace SDI_Api.Application.Auth.Validators;

public class GenerateTwoFactorKeyQueryValidator : AbstractValidator<GenerateTwoFactorKeyQuery>
{
    public GenerateTwoFactorKeyQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID cannot be empty.");
    }
}

using SDI_Api.Application.Company.Commands;

namespace SDI_Api.Application.Company.Validators;

public class AddUserToCompanyCommandValidator : AbstractValidator<AddUserToCompanyCommand>
{
    public AddUserToCompanyCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(v => v.UserData)
            .NotNull().WithMessage("User data is required.");

        RuleFor(v => v.UserData.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");
    }
}


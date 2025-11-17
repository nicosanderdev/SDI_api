using SDI_Api.Application.Company.Commands;

namespace SDI_Api.Application.Company.Validators;

public class RemoveUserFromCompanyCommandValidator : AbstractValidator<RemoveUserFromCompanyCommand>
{
    public RemoveUserFromCompanyCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(v => v.UserToRemoveId)
            .NotEmpty().WithMessage("User to remove ID is required.")
            .NotEqual(v => v.UserId).WithMessage("Cannot remove yourself from the company.");
    }
}


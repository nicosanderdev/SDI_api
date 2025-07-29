using SDI_Api.Application.EstateProperties.Queries;

namespace SDI_Api.Application.EstateProperties.Validations;

public class GetUsersEstatePropertyQueryValidator : AbstractValidator<GetUsersEstatePropertyQuery>
{
    public GetUsersEstatePropertyQueryValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty()
            .WithMessage("Property ID is required.")
            .Must(id => id != Guid.Empty)
            .WithMessage("Property ID cannot be an empty GUID.");
    }
}

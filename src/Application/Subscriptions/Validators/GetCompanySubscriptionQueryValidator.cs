using SDI_Api.Application.Subscriptions.Queries;

namespace SDI_Api.Application.Subscriptions.Validators;

public class GetCompanySubscriptionQueryValidator : AbstractValidator<GetCompanySubscriptionQuery>
{
    public GetCompanySubscriptionQueryValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Company ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}


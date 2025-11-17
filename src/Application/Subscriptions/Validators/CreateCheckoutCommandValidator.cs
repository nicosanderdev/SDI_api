using SDI_Api.Application.Subscriptions.Commands;

namespace SDI_Api.Application.Subscriptions.Validators;

public class CreateCheckoutCommandValidator : AbstractValidator<CreateCheckoutCommand>
{
    public CreateCheckoutCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Checkout request is required.");

        RuleFor(x => x.Request.PlanId)
            .NotEmpty()
            .WithMessage("Plan ID is required.")
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("Plan ID must be a valid GUID.");

        RuleFor(x => x.Request.CompanyId)
            .Must((command, companyId) => 
                !command.Request.IsCompanySubscription || 
                (!string.IsNullOrEmpty(companyId) && Guid.TryParse(companyId, out _)))
            .WithMessage("Company ID is required and must be a valid GUID when creating a company subscription.");
    }
}


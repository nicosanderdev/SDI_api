using SDI_Api.Application.Subscriptions.Commands;

namespace SDI_Api.Application.Subscriptions.Validators;

public class ChangePlanCommandValidator : AbstractValidator<ChangePlanCommand>
{
    public ChangePlanCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Change plan request is required.");

        RuleFor(x => x.Request.PlanId)
            .NotEmpty()
            .WithMessage("Plan ID is required.")
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("Plan ID must be a valid GUID.");
    }
}


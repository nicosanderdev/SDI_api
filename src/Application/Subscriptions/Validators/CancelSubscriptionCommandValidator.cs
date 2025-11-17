using SDI_Api.Application.Subscriptions.Commands;

namespace SDI_Api.Application.Subscriptions.Validators;

public class CancelSubscriptionCommandValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Cancel subscription request is required.");
    }
}


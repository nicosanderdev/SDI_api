using SDI_Api.Application.Subscriptions.Commands;

namespace SDI_Api.Application.Subscriptions.Validators;

public class CreateManualInvoiceCommandValidator : AbstractValidator<CreateManualInvoiceCommand>
{
    public CreateManualInvoiceCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Manual invoice request is required.");

        RuleFor(x => x.Request.SubscriptionId)
            .NotEmpty()
            .WithMessage("Subscription ID is required.")
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("Subscription ID must be a valid GUID.");

        RuleFor(x => x.Request.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Request.Currency)
            .NotEmpty()
            .WithMessage("Currency is required.")
            .Length(3)
            .WithMessage("Currency must be a 3-character code (e.g., USD).");

        RuleFor(x => x.Request.TrialDays)
            .GreaterThan(0)
            .When(x => x.Request.GrantTrial)
            .WithMessage("Trial days must be greater than zero when granting a trial.");
    }
}


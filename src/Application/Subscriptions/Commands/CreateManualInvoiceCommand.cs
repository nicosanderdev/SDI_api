using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Subscriptions;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Enums;

namespace SDI_Api.Application.Subscriptions.Commands;

public class CreateManualInvoiceCommand : IRequest<BillingHistoryDto>
{
    public ManualInvoiceRequestDto Request { get; set; } = null!;
}

public class CreateManualInvoiceCommandHandler : IRequestHandler<CreateManualInvoiceCommand, BillingHistoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateManualInvoiceCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<BillingHistoryDto> Handle(CreateManualInvoiceCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Request.SubscriptionId, out var subscriptionId))
        {
            throw new ArgumentException("Invalid subscription ID format.");
        }

        var subscription = await _context.Subscriptions
            .FindAsync(new object[] { subscriptionId }, cancellationToken);

        if (subscription == null)
        {
            throw new NotFoundException(nameof(Subscription), request.Request.SubscriptionId);
        }

        if (request.Request.GrantTrial && request.Request.TrialDays.HasValue)
        {
            // Grant trial period
            subscription.Status = SubscriptionStatus.trialing;
            var trialEndDate = DateTime.UtcNow.AddDays(request.Request.TrialDays.Value);
            subscription.CurrentPeriodStart = DateTime.UtcNow;
            subscription.CurrentPeriodEnd = trialEndDate;
            subscription.UpdatedAt = DateTime.UtcNow;
        }

        // Create billing history entry
        var billingHistory = new BillingHistory
        {
            SubscriptionId = subscriptionId,
            Amount = request.Request.Amount,
            Currency = request.Request.Currency,
            Status = InvoiceStatus.open,
            ProviderInvoiceId = $"manual_{Guid.NewGuid()}",
            Created = DateTime.UtcNow
        };

        _context.BillingHistories.Add(billingHistory);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<BillingHistoryDto>(billingHistory);
    }
}


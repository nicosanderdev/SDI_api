using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Subscriptions;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Enums;

namespace SDI_Api.Application.Subscriptions.Commands;

public class ProcessWebhookCommand : IRequest<bool>
{
    public string WebhookId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public JsonElement EventData { get; set; }
    public string Signature { get; set; } = string.Empty;
    public string RawBody { get; set; } = string.Empty;
}

public class ProcessWebhookCommandHandler : IRequestHandler<ProcessWebhookCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public ProcessWebhookCommandHandler(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<bool> Handle(ProcessWebhookCommand request, CancellationToken cancellationToken)
    {
        // Verify signature
        if (!VerifyWebhookSignature(request.Signature, request.RawBody))
        {
            throw new UnauthorizedAccessException("Invalid webhook signature.");
        }

        // Check for idempotency - prevent processing same webhook twice
        var existingWebhook = await _context.WebhookEvents
            .FirstOrDefaultAsync(w => w.ProviderEventId == request.WebhookId, cancellationToken);

        if (existingWebhook != null)
        {
            // Webhook already processed
            return true;
        }

        // Store webhook event for idempotency
        var webhookEvent = new WebhookEvent
        {
            ProviderEventId = request.WebhookId,
            EventType = request.EventType,
            Processed = false,
            Created = DateTime.UtcNow
        };

        _context.WebhookEvents.Add(webhookEvent);

        // Process webhook events
        switch (request.EventType)
        {
            case "invoice.paid":
                await ProcessInvoicePaid(request.EventData, cancellationToken);
                break;
            case "invoice.failed":
                await ProcessInvoiceFailed(request.EventData, cancellationToken);
                break;
            case "subscription.updated":
                await ProcessSubscriptionUpdated(request.EventData, cancellationToken);
                break;
            case "checkout.session.completed":
                await ProcessCheckoutCompleted(request.EventData, cancellationToken);
                break;
            case "payment.refunded":
                await ProcessPaymentRefunded(request.EventData, cancellationToken);
                break;
            default:
                // Unknown event type, log but don't fail
                break;
        }

        webhookEvent.Processed = true;
        webhookEvent.ProcessedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private bool VerifyWebhookSignature(string signature, string rawBody)
    {
        // TODO: Implement signature verification based on payment provider
        // For Stripe: Use webhook secret to compute HMAC SHA256
        // For PayPal: Use signature verification
        // For now, return true (implement based on your payment provider)
        
        var webhookSecret = _configuration["PaymentProvider:WebhookSecret"];
        if (string.IsNullOrEmpty(webhookSecret))
        {
            // In development, skip verification if secret is not configured
            return true;
        }

        // Example for Stripe-style signature verification:
        // var expectedSignature = ComputeHmacSha256(webhookSecret, rawBody);
        // return signature == expectedSignature;

        return true;
    }

    private async Task ProcessInvoicePaid(JsonElement eventData, CancellationToken cancellationToken)
    {
        // Extract invoice data from webhook payload
        // This is provider-specific, adjust based on your payment provider
        if (!eventData.TryGetProperty("id", out var idElement) || 
            !eventData.TryGetProperty("subscription", out var subscriptionElement))
            return;

        var invoiceId = idElement.GetString();
        var subscriptionId = subscriptionElement.GetString();
        
        if (string.IsNullOrEmpty(invoiceId) || string.IsNullOrEmpty(subscriptionId))
            return;

        var amount = eventData.TryGetProperty("amount_paid", out var amountElement) 
            ? amountElement.GetDecimal() 
            : eventData.TryGetProperty("amount", out var amountAlt) 
                ? amountAlt.GetDecimal() 
                : 0;
        var currency = eventData.TryGetProperty("currency", out var currencyElement) 
            ? currencyElement.GetString() ?? "USD" 
            : "USD";

        // Find subscription by provider ID
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.ProviderSubscriptionId == subscriptionId, cancellationToken);

        if (subscription == null)
            return;

        // Update or create billing history
        var billingHistory = await _context.BillingHistories
            .FirstOrDefaultAsync(bh => bh.ProviderInvoiceId == invoiceId, cancellationToken);

        if (billingHistory == null)
        {
            billingHistory = new BillingHistory
            {
                SubscriptionId = subscription.Id,
                ProviderInvoiceId = invoiceId,
                Amount = amount / 100, // Convert from cents to dollars
                Currency = currency,
                Status = InvoiceStatus.paid,
                PaidAt = DateTime.UtcNow,
                Created = DateTime.UtcNow
            };
            _context.BillingHistories.Add(billingHistory);
        }
        else
        {
            billingHistory.Status = InvoiceStatus.paid;
            billingHistory.PaidAt = DateTime.UtcNow;
        }

        // Update subscription status if needed
        if (subscription.Status == SubscriptionStatus.past_due)
        {
            subscription.Status = SubscriptionStatus.active;
            subscription.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task ProcessInvoiceFailed(JsonElement eventData, CancellationToken cancellationToken)
    {
        if (!eventData.TryGetProperty("id", out var idElement) || 
            !eventData.TryGetProperty("subscription", out var subscriptionElement))
            return;

        var invoiceId = idElement.GetString();
        var subscriptionId = subscriptionElement.GetString();

        if (string.IsNullOrEmpty(invoiceId) || string.IsNullOrEmpty(subscriptionId))
            return;

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.ProviderSubscriptionId == subscriptionId, cancellationToken);

        if (subscription == null)
            return;

        var billingHistory = await _context.BillingHistories
            .FirstOrDefaultAsync(bh => bh.ProviderInvoiceId == invoiceId, cancellationToken);

        if (billingHistory != null)
        {
            billingHistory.Status = InvoiceStatus.uncollectible;
        }

        subscription.Status = SubscriptionStatus.past_due;
        subscription.UpdatedAt = DateTime.UtcNow;
    }

    private async Task ProcessSubscriptionUpdated(JsonElement eventData, CancellationToken cancellationToken)
    {
        if (!eventData.TryGetProperty("id", out var idElement))
            return;

        var subscriptionId = idElement.GetString();
        if (string.IsNullOrEmpty(subscriptionId))
            return;

        var status = eventData.TryGetProperty("status", out var statusElement) 
            ? statusElement.GetString() 
            : null;
        var currentPeriodStart = eventData.TryGetProperty("current_period_start", out var startElement) 
            ? startElement.GetInt64() 
            : 0;
        var currentPeriodEnd = eventData.TryGetProperty("current_period_end", out var endElement) 
            ? endElement.GetInt64() 
            : 0;
        var cancelAtPeriodEnd = eventData.TryGetProperty("cancel_at_period_end", out var cancelElement) 
            ? cancelElement.GetBoolean() 
            : false;

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.ProviderSubscriptionId == subscriptionId, cancellationToken);

        if (subscription == null)
            return;

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<SubscriptionStatus>(status, true, out var statusEnum))
        {
            subscription.Status = statusEnum;
        }

        if (currentPeriodStart > 0)
            subscription.CurrentPeriodStart = DateTimeOffset.FromUnixTimeSeconds(currentPeriodStart).UtcDateTime;
        if (currentPeriodEnd > 0)
            subscription.CurrentPeriodEnd = DateTimeOffset.FromUnixTimeSeconds(currentPeriodEnd).UtcDateTime;
        subscription.CancelAtPeriodEnd = cancelAtPeriodEnd;
        subscription.UpdatedAt = DateTime.UtcNow;
    }

    private Task ProcessCheckoutCompleted(JsonElement eventData, CancellationToken cancellationToken)
    {
        // Handle checkout session completed
        // Create or update subscription based on checkout session data
        var sessionId = eventData.GetProperty("id").GetString();
        var customerId = eventData.GetProperty("customer").GetString();
        var subscriptionId = eventData.GetProperty("subscription").GetString();

        // TODO: Implement based on your checkout flow
        // This would typically create a new subscription or activate a trial
        return Task.CompletedTask;
    }

    private async Task ProcessPaymentRefunded(JsonElement eventData, CancellationToken cancellationToken)
    {
        if (!eventData.TryGetProperty("invoice", out var invoiceElement))
            return;

        var invoiceId = invoiceElement.GetString();
        if (string.IsNullOrEmpty(invoiceId))
            return;

        var billingHistory = await _context.BillingHistories
            .FirstOrDefaultAsync(bh => bh.ProviderInvoiceId == invoiceId, cancellationToken);

        if (billingHistory != null)
        {
            // Update billing history to reflect refund
            // You might want to create a separate refund record instead
            billingHistory.Status = InvoiceStatus.voided;
        }
    }
}



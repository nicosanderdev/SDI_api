namespace SDI_Api.Application.DTOs.Subscriptions;

public class ManualInvoiceRequestDto
{
    public string SubscriptionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public bool GrantTrial { get; set; } = false;
    public int? TrialDays { get; set; }
}


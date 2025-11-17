namespace SDI_Api.Application.DTOs.Subscriptions;

public class CheckoutRequestDto
{
    public string PlanId { get; set; } = string.Empty;
    public bool IsCompanySubscription { get; set; }
    public string? CompanyId { get; set; }
}


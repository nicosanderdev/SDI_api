using SDI_Api.Domain.Enums;

namespace SDI_Api.Application.DTOs.Subscriptions;

public class SubscriptionDto
{
    public string Id { get; set; } = string.Empty;
    public OwnerType OwnerType { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public string? ProviderCustomerId { get; set; }
    public string? ProviderSubscriptionId { get; set; }
    public string PlanId { get; set; } = string.Empty;
    public PlanDto Plan { get; set; } = null!;
    public SubscriptionStatus Status { get; set; }
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}


using SDI_Api.Domain.Enums;

namespace SDI_Api.Application.DTOs.Subscriptions;

public class PlanDto
{
    public string Id { get; set; } = string.Empty;
    public PlanKey Key { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public int? MaxProperties { get; set; }
    public int? MaxUsers { get; set; }
    public int? MaxStorageMb { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public bool IsActive { get; set; }
}


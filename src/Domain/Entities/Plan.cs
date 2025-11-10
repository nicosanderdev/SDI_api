using System.ComponentModel.DataAnnotations;
using SDI_Api.Domain.Enums;

namespace SDI_Api.Domain.Entities;

public class Plan : BaseAuditableEntity
{
    [Required]
    public PlanKey Key { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public decimal MonthlyPrice { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";
    
    public int? MaxProperties { get; set; }
    
    public int? MaxUsers { get; set; }
    
    public int? MaxStorageMb { get; set; }
    
    [Required]
    public BillingCycle BillingCycle { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    
    public Plan()
    {
        Id = Guid.NewGuid();
    }
}


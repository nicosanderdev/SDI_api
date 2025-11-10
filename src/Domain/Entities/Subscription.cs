using System.ComponentModel.DataAnnotations;
using SDI_Api.Domain.Enums;

namespace SDI_Api.Domain.Entities;

public class Subscription : BaseAuditableEntity
{
    [Required]
    public OwnerType OwnerType { get; set; }
    
    [Required]
    public Guid OwnerId { get; set; } // FK to either ApplicationUser or Company, depending on OwnerType
    
    [MaxLength(255)]
    public string? ProviderCustomerId { get; set; }
    
    [MaxLength(255)]
    public string? ProviderSubscriptionId { get; set; }
    
    [Required]
    public Guid PlanId { get; set; } // FK to Plan
    
    [Required]
    public SubscriptionStatus Status { get; set; }
    
    [Required]
    public DateTime CurrentPeriodStart { get; set; }
    
    [Required]
    public DateTime CurrentPeriodEnd { get; set; }
    
    [Required]
    public bool CancelAtPeriodEnd { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Plan Plan { get; set; } = null!;
    public ICollection<BillingHistory> BillingHistories { get; set; } = new List<BillingHistory>();
    
    public Subscription()
    {
        Id = Guid.NewGuid();
    }
}


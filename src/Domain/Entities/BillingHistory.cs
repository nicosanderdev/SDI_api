using System.ComponentModel.DataAnnotations;
using SDI_Api.Domain.Enums;

namespace SDI_Api.Domain.Entities;

public class BillingHistory : BaseAuditableEntity
{
    [Required]
    public Guid SubscriptionId { get; set; } // FK to Subscription
    
    [MaxLength(255)]
    public string? ProviderInvoiceId { get; set; }
    
    [Required]
    public decimal Amount { get; set; } // in cents
    
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";
    
    [Required]
    [MaxLength(50)]
    public InvoiceStatus Status { get; set; } = InvoiceStatus.open;
    
    public DateTime? PaidAt { get; set; }
    
    public Subscription Subscription { get; set; } = null!;
    
    public BillingHistory()
    {
        Id = Guid.NewGuid();
    }
}


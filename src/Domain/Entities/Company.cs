using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Domain.Entities;

public class Company : BaseAuditableEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public Guid BillingContactUserId { get; set; } // FK to ApplicationUser
    
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string BillingEmail { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(2048)]
    public string? LogoUrl { get; set; }
    
    [MaxLength(2048)]
    public string? BannerUrl { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(255)]
    public string? Street { get; set; }
    
    [MaxLength(255)]
    public string? Street2 { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(100)]
    public string? State { get; set; }
    
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    [MaxLength(50)]
    public string? Phone { get; set; }
    
    // Navigation properties
    public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    
    public Company()
    {
        Id = Guid.NewGuid();
    }
}


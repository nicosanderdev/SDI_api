using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Domain.Entities;

public class PropertyVisitLog : BaseAuditableEntity
{
    public Guid PropertyId { get; set; }
    public virtual EstateProperty? Property { get; set; }
    public DateTime VisitedOnUtc { get; set; }
    [MaxLength(50)]
    public string? Source { get; set; } // e.g., "Web", "App", "Referral"
    // public Guid? UserId { get; set; } // Optional: if tracking user visits
    // public virtual ApplicationUser? User { get; set; } // Optional: Navigation

    public PropertyVisitLog()
    {
        Id = Guid.NewGuid();
        VisitedOnUtc = DateTime.UtcNow;
    }
}

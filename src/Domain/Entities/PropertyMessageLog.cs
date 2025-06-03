namespace SDI_Api.Domain.Entities;

public class PropertyMessageLog : BaseAuditableEntity
{
    public Guid PropertyId { get; set; }
    public virtual EstateProperty? Property { get; set; } // Optional: Navigation
    public DateTime SentOnUtc { get; set; }
    // public Guid? UserId { get; set; } // Optional: if tracking user who sent/received message
    // public virtual ApplicationUser? User { get; set; } // Optional: Navigation
    // public string MessageContentPreview { get; set; } // Optional

    public PropertyMessageLog()
    {
        Id = Guid.NewGuid();
        SentOnUtc = DateTime.UtcNow;
    }
}

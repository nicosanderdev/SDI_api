using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Domain.Entities;

public class MessageThread : BaseAuditableEntity
{
    [Required]
    [MaxLength(255)]
    public string Subject { get; set; } = string.Empty;

    public Guid? PropertyId { get; set; } // Optional: Link to an EstateProperty
    public virtual EstateProperty? Property { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastMessageAtUtc { get; set; }

    // Participants: Could be implicitly defined by messages or explicitly tracked
    // For simplicity, we'll infer participants from Messages
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public MessageThread()
    {
        Id = Guid.NewGuid();
        CreatedAtUtc = DateTime.UtcNow;
        LastMessageAtUtc = DateTime.UtcNow;
    }
}

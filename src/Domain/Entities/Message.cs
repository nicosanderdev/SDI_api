using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDI_Api.Domain.Entities;

public class Message : BaseAuditableEntity
{
    public Guid ThreadId { get; set; }
    public virtual MessageThread Thread { get; set; } = null!;

    public Guid SenderId { get; set; }
    public virtual ApplicationUser Sender { get; set; } = null!;

    [Required]
    public string Body { get; set; } = string.Empty; // Full message content

    [MaxLength(200)] // Snippet for list views
    public string Snippet { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    // This links a message to its recipients and their individual statuses
    public virtual ICollection<MessageRecipient> MessageRecipients { get; set; } = new List<MessageRecipient>();
        
    public Guid? InReplyToMessageId { get; set; } // Original message this is a reply to
    [ForeignKey("InReplyToMessageId")]
    public virtual Message? InReplyToMessage { get; set; }


    public Message()
    {
        Id = Guid.NewGuid();
        CreatedAtUtc = DateTime.UtcNow;
    }
}

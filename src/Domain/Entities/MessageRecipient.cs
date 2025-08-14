namespace SDI_Api.Domain.Entities;

public class MessageRecipient : BaseAuditableEntity
{
    public Guid MessageId { get; set; }
    public virtual Message Message { get; set; } = null!;
    public Guid RecipientId { get; set; }
    public Member Recipient { get; set; } = null!;
    public DateTime ReceivedAtUtc { get; set; }
    public bool IsRead { get; set; }
    public bool HasBeenRepliedToByRecipient { get; set; }
    public bool IsStarred { get; set; }
    public bool IsArchived { get; set; }

    public MessageRecipient()
    {
        Id = Guid.NewGuid();
        ReceivedAtUtc = DateTime.UtcNow;
    }
}

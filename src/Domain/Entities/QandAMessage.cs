namespace SDI_Api.Domain.Entities;

public class QandAMessage : BaseEntity
{
    public Guid ThreadId { get; set; }
    public required QandAMessageThread Thread { get; set; }
    public required string Content { get; set; }
    public DateTime SentAt { get; set; }
    public string? SenderName { get; set; }
    public string? SenderEmail { get; set; }
    /*public ApplicationUser Sender { get; set; }*/
}

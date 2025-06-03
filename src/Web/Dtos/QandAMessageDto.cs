namespace SDI_Api.Web.Dtos;

public class QandAMessageDto
{
    public Guid? EstatePropertyId { get; set; }
    public Guid? MessageThreadId { get; set; }
    public string? Title { get; set; }
    public required string Content { get; set; }
    public DateTime SentAt { get; set; }
    public required string SenderName { get; set; }
}

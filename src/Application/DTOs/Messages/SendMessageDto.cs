using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Application.DTOs.Messages;

public class SendMessageDto
{
    public string? RecipientId { get; set; }
    public string? PropertyId { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? InReplyToMessageId { get; set; }
    public string? ThreadId { get; set; }
}

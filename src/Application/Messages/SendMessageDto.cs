using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Application.Messages;

public class SendMessageDto
{
    [Required]
    public string RecipientId { get; set; } = string.Empty; // Target user's ID
    public string? PropertyId { get; set; }
    [Required]
    public string Subject { get; set; } = string.Empty; // Used if creating a new thread
    [Required]
    public string Body { get; set; } = string.Empty;
    public string? InReplyToMessageId { get; set; } // ID of the message being replied to
    public string? ThreadId { get; set; } // Existing thread ID, if replying
}

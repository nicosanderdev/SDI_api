namespace Sdi_Api.Application.DTOs.Messages;

public class MessageDto
{
    public string Id { get; set; } = string.Empty;
    public string? ThreadId { get; set; }
    public string? SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderEmail { get; set; } // Optional
    public string? RecipientId { get; set; } // Current user's ID as recipient
    public string? PropertyId { get; set; }
    public string? PropertyTitle { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Snippet { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty; // ISO date string
    public bool IsRead { get; set; }
    public bool IsReplied { get; set; } // Renamed from HasBeenRepliedToByRecipient for DTO
    public bool IsStarred { get; set; }
    public bool IsArchived { get; set; }
}

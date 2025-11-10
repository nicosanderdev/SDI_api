using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Domain.Entities;

public class WebhookEvent : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string ProviderEventId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;
    
    public bool Processed { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public DateTime Created { get; set; } = DateTime.UtcNow;
}


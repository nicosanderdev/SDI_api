using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Domain.Entities;

public class RecoveryCode
{
    public Guid RecoveryCodeId { get; set; }
    public required string Code { get; set; }
    public required Guid UserId { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UsedAt { get; set; }
}

using System.ComponentModel.DataAnnotations;
using SDI_Api.Domain.Enums;

namespace SDI_Api.Domain.Entities;

public class Usage : BaseEntity
{
    [Required]
    public OwnerType OwnerType { get; set; }
    
    // Member or company
    [Required]
    public Guid OwnerId { get; set; }
    
    [Required]
    public int PropertiesCount { get; set; }
    
    [Required]
    public int StorageUsedMb { get; set; }
    
    [Required]
    public DateTime SnapshotAt { get; set; } = DateTime.UtcNow;
    
    public Usage()
    {
        Id = Guid.NewGuid();
    }
}


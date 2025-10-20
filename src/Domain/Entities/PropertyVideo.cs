using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Domain.Entities;

public class PropertyVideo : BaseAuditableEntity
{
    [Required]
    [MaxLength(2048)]
    public string Url { get; set; } = string.Empty;
    [MaxLength(50)]
    public string? Title { get; set; }
    [MaxLength(255)]
    public string? Description { get; set; }
    public Guid EstatePropertyId { get; set; }
    public virtual EstateProperty EstateProperty { get; set; } = null!;
}

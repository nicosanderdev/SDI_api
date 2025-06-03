using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Domain.Entities;

public class PropertyImage : BaseAuditableEntity
{
    [Required]
    [MaxLength(2048)]
    public string Url { get; set; } = string.Empty;
    [MaxLength(255)]
    public string? AltText { get; set; }
    public bool IsMain { get; set; } // Changed from bool? to bool, default false

    public Guid EstatePropertyId { get; set; }
    public virtual EstateProperty EstateProperty { get; set; } = null!;

    public PropertyImage()
    {
        Id = Guid.NewGuid();
    }
}

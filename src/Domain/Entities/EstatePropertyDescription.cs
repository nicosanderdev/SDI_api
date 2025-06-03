using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Domain.Entities;

public class EstatePropertyDescription : BaseAuditableEntity
{
    [MaxLength(10)]
    public string? LanguageCode { get; set; } // e.g., "en", "es"
    [MaxLength(200)]
    public string? Title { get; set; }
    [Required]
    public string Text { get; set; } = string.Empty;

    public Guid EstatePropertyId { get; set; }
    public virtual EstateProperty EstateProperty { get; set; } = null!;

    public EstatePropertyDescription()
    {
        Id = Guid.NewGuid();
    }
}

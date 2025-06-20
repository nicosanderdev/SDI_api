using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Domain.Entities;

public class EstateProperty : BaseAuditableEntity
{
    [MaxLength(255)]
    public string? Address { get; set; }
    [MaxLength(255)]
    public string? Address2 { get; set; }
    [MaxLength(100)]
    public string? City { get; set; }
    [MaxLength(100)]
    public string? State { get; set; }
    [MaxLength(20)]
    public string? ZipCode { get; set; }
    [MaxLength(100)]
    public string? Country { get; set; }
    public bool IsPublic { get; set; } = true;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public decimal Price { get; set; } // Store as decimal
    public PropertyStatus Status { get; set; }
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty; // e.g., "Apartamento", "Casa"
    public decimal AreaValue { get; set; } // e.g., 95
    [MaxLength(10)]
    public string AreaUnit { get; set; } = "m²"; // e.g., "m²"

    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
    public int? Visits { get; set; }

    public Guid? MainImageId { get; set; }
    public virtual PropertyImage? MainImage { get; set; }
    public virtual ICollection<PropertyImage> PropertyImages { get; set; } = new List<PropertyImage>();

    public Guid? FeaturedDescriptionId { get; set; }
    public virtual EstatePropertyDescription? FeaturedDescription { get; set; }
    public virtual ICollection<EstatePropertyDescription> EstatePropertyDescriptions { get; set; } = new List<EstatePropertyDescription>();
    
    public Guid? OwnerId { get; set; }
    public Member Owner { get; set; } = null!;

    // Constructor
    public EstateProperty()
    {
        Id = Guid.NewGuid();
    }
}

using System.ComponentModel.DataAnnotations;
using System.Data.Common;

namespace SDI_Api.Domain.Entities;

public class EstateProperty : BaseAuditableEntity
{
    // Address
    [MaxLength(255)]
    public string? StreetName { get; set; }
    [MaxLength(25)]
    public string? HouseNumber { get; set; }
    [MaxLength(100)]
    public string? Neighborhood { get; set; }
    [MaxLength(100)]
    public string? City { get; set; }
    [MaxLength(100)]
    public string? State { get; set; }
    [MaxLength(20)]
    public string? ZipCode { get; set; }
    [MaxLength(100)]
    public string? Country { get; set; }
    public decimal LocationLatitude { get; set; }
    public decimal LocationLongitude { get; set; }

    // Property description
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "";
    [MaxLength(100)]
    public PropertyType Type { get; set; }
    public decimal AreaValue { get; set; } // e.g., 95
    [MaxLength(10)]
    public AreaUnit AreaUnit { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public bool HasGarage { get; set; }
    public int GarageSpaces { get; set; } = 0;
    // Other info
    public int? Visits { get; set; }
    // Relationships
    public List<PropertyDocument> Documents { get; set; } = new List<PropertyDocument>();
    public Guid? MainImageId { get; set; }
    public virtual ICollection<PropertyImage> PropertyImages { get; set; } = new List<PropertyImage>();
    public virtual ICollection<EstatePropertyValues> EstatePropertyValues { get; set; } = new List<EstatePropertyValues>();
    public Guid? OwnerId { get; set; }
    public Member Owner { get; set; } = null!;

    // Constructor
    public EstateProperty()
    {
        Id = Guid.NewGuid();
    }
}

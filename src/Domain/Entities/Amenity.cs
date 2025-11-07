namespace SDI_Api.Domain.Entities;

public class Amenity : BaseEntity
{
    public string? Name { get; set; }
    public string? IconId { get; set; }
    public virtual ICollection<EstatePropertyAmenity> EstatePropertyAmenities { get; set; } = new List<EstatePropertyAmenity>();
}

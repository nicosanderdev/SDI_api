namespace SDI_Api.Domain.Entities;

public class EstatePropertyAmenity
{
    public Guid EstatePropertyId { get; set; }
    public EstateProperty EstateProperty { get; set; } = default!;

    public Guid AmenityId { get; set; }
    public Amenity Amenity { get; set; } = default!;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; } = null;

    public EstatePropertyAmenity()
    {
        CreatedAtUtc = DateTimeOffset.UtcNow;
        DeletedAtUtc = null;
    }
}

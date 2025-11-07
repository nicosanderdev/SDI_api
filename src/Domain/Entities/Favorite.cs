namespace SDI_Api.Domain.Entities;

public class Favorite
{
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public Guid EstatePropertyId { get; set; }
    public EstateProperty EstateProperty { get; set; } = null!;

    public DateTimeOffset FavoritedAt { get; set; }
}

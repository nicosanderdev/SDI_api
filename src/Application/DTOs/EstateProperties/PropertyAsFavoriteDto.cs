namespace SDI_Api.Application.DTOs.EstateProperties;

public class PropertyAsFavoriteDto
{
    public Guid EstatePropertyId { get; set; }
    public Guid? UserId { get; set; }
    public bool IsFavorite { get; set; }
}

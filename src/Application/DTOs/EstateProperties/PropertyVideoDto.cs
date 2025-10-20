namespace SDI_Api.Application.Dtos;

public class PropertyVideoDto
{
    public string? Id { get; set; }
    public string? Url { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Guid EstatePropertyId { get; set; }
    public bool IsPublic { get; set; } = true;
}

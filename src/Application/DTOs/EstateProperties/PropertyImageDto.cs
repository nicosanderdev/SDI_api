using Microsoft.AspNetCore.Http;

namespace SDI_Api.Application.Dtos;

public class PropertyImageDto
{
    public string? Id { get; set; }
    public string? Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool? IsMain { get; set; }
    public Guid EstatePropertyId { get; set; }
    public IFormFile? File { get; set; }
    public bool IsPublic { get; set; } = true;
}

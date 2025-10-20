using Microsoft.AspNetCore.Http;

namespace SDI_Api.Application.DTOs.EstateProperties;

public class PropertyDocumentDto
{
    public string? Id { get; set; }
    public string? Url { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? FileName { get; set; }

    public Guid EstatePropertyId { get; set; }
    public IFormFile? File { get; set; }
    public bool IsPublic { get; set; } = true;
}

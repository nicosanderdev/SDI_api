namespace SDI_Api.Application.EstateProperties.Commands;

public class CreateOrUpdatePropertyImageDto
{
    public string? Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool? IsMain { get; set; }
    public Guid EstatePropertyId { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public required byte[] ImageData { get; set; }
    public bool IsPublic { get; set; } = true;
}

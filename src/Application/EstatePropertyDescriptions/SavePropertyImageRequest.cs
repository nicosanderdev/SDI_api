namespace SDI_Api.Application.EstatePropertyDescriptions;

public class SavePropertyImageRequest
{
    public required Guid EstatePropertyId { get; set; }

    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public required byte[] ImageData { get; set; }
    public bool IsPublic { get; set; } = true;
}

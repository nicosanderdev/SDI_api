namespace SDI_Api.Domain.Entities;

public class PropertyDocument : BaseEntity
{
    public string? Name { get; set; }
    public string? FileType { get; set; }
    public string? Url { get; set; }
    public FileInfo? FileInfo { get; set; }
}

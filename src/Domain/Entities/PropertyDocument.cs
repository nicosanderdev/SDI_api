namespace SDI_Api.Domain.Entities;

public class PropertyDocument : BaseEntity
{
    public string? Name { get; set; }
    // public string? FileName { get; set; }
    public string? FileType { get; set; }
    public string? Url { get; set; }
    public Guid EstatePropertyId { get; set; }
    public virtual EstateProperty EstateProperty { get; set; } = null!;
    public bool IsPublic { get; set; } = true;
}

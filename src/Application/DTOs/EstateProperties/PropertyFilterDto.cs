namespace SDI_Api.Application.DTOs.EstateProperties;

public class PropertyFilterDto
{
    public bool? IsDeleted { get; set; }
    public string? OwnerId { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }

    // Optional bounding box coordinates (south-west and north-east corners)
    public float? SwLat { get; set; }
    public float? SwLng { get; set; }
    public float? NeLat { get; set; }
    public float? NeLng { get; set; }
}

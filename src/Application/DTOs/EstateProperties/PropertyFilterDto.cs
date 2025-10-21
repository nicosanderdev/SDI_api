namespace SDI_Api.Application.DTOs.EstateProperties;

public class PropertyFilterDto
{
    // Used only on users endpoints
    public bool IsDeleted { get; set; } = false;
    // Used on public and private endpoints
    public string? OwnerId { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IncludeImages { get; set; } = false;
    public bool? IncludeVideos { get; set; } = false;
    public bool? IncludeDocuments { get; set; } = false;
    public bool? IncludeAmenities { get; set; } = false;
}

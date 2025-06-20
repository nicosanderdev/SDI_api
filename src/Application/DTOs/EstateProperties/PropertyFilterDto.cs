namespace SDI_Api.Application.DTOs.EstateProperties;

public class PropertyFilterDto
{
    public bool? IsDeleted { get; set; }
    public string? OwnerId { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }
}

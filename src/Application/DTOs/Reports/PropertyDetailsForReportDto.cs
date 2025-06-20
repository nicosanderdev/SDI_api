namespace SDI_Api.Application.DTOs.Reports;
public class PropertyDetailsForReportDto // Subset of EstatePropertyDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Address { get; set; }
    // Add other relevant PropertyData fields if needed by the report
    public string? Price { get; set; }
    public string? Status { get; set; }
    public string? Type {get; set;}
}

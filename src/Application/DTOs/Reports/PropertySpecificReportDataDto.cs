namespace SDI_Api.Application.DTOs;

public class PropertySpecificReportDataDto
{
    public PropertyDetailsForReportDto PropertyDetails { get; set; } = new PropertyDetailsForReportDto();
    public List<DateCountDto> VisitTrend { get; set; } = new List<DateCountDto>();
    public List<DateCountDto> MessageTrend { get; set; } = new List<DateCountDto>();
    public decimal? ConversionRate { get; set; } // e.g., 0.13 for 13%
    public string? AverageTimeToRespond { get; set; } // e.g., "2 hours"
}

namespace SDI_Api.Application.DTOs.Reports;

public class DashboardSummaryDataDto
{
    public DashboardSummaryStatDto Visits { get; set; } = new DashboardSummaryStatDto();
    public DashboardSummaryStatDto Messages { get; set; } = new DashboardSummaryStatDto();
    public DashboardSummaryStatDto? TotalProperties { get; set; } // Assuming this can also have a trend
    public DashboardSummaryStatDto? ConversionRate { get; set; }
}

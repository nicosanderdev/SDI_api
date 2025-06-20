namespace SDI_Api.Application.DTOs.Reports;
public class DashboardSummaryStatDto
{
    public long CurrentPeriod { get; set; } // Changed to long
    public decimal? PercentageChange { get; set; }
    public string? ChangeDirection { get; set; } // "increase" | "decrease" | "neutral"
}


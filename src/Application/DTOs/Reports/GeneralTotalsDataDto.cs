namespace SDI_Api.Application.DTOs;

public class GeneralTotalsDataDto
{
    public int TotalProperties { get; set; }
    public long TotalVisitsLifetime { get; set; } // Changed to long for potentially large numbers
    public long TotalMessagesLifetime { get; set; } // Changed to long
    public int? ActiveListings { get; set; }
    public decimal? AveragePrice { get; set; } // Use decimal for currency
}

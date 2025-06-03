namespace SDI_Api.Application.DTOs;

public class PropertyVisitStatDto
{
    public string PropertyId { get; set; } = string.Empty;
    public string PropertyTitle { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int VisitCount { get; set; }
    public string? Price { get; set; }
    public string? Status { get; set; }
    public int? Messages { get; set; }
    public string? MessagesTrend { get; set; } // 'up' | 'down' | 'flat'
    public string? VisitsTrend { get; set; }   // 'up' | 'down' | 'flat'
    public string? Conversion { get; set; }    // e.g., '13%'
    public string? ConversionTrend { get; set; } // 'up' | 'down' | 'flat'
}

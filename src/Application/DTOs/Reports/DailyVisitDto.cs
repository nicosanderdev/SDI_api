namespace SDI_Api.Application.DTOs;

public class DailyVisitDto
{
    public string Date { get; set; } = string.Empty; // "yyyy-MM-dd" or "dd/MM"
    public string? DayName { get; set; } // "Mon", "Tue", etc.
    public int Visits { get; set; }
}

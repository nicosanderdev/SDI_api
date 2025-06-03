namespace SDI_Api.Application.DTOs;

public class MonthlySummaryDataDto
{
    public List<DateCountDto> Visits { get; set; } = new List<DateCountDto>();
    public List<DateCountDto> Messages { get; set; } = new List<DateCountDto>();
}

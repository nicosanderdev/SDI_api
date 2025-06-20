namespace SDI_Api.Application.DTOs.Reports;
public class VisitsByPropertyDataDto // Corresponds to VisitsByPropertyParams for request
{
    public List<PropertyVisitStatDto> Data { get; set; } = new List<PropertyVisitStatDto>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
}

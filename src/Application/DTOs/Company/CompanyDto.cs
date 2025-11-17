namespace SDI_Api.Application.DTOs.Company;

public class CompanyDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public AddressDto? Address { get; set; }
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; }
    public CompanyStatisticsDto? Statistics { get; set; }
    public List<CompanyUserDto>? Users { get; set; }
}

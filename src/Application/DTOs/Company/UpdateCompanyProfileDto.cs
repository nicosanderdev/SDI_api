namespace SDI_Api.Application.DTOs.Company;

public class UpdateCompanyProfileDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public AddressDto? Address { get; set; }
}


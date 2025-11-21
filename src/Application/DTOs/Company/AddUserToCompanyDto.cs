namespace SDI_Api.Application.DTOs.Company;

public class AddUserToCompanyDto
{
    public required string Email { get; set; }
    public required string CompanyId { get; set; }
}


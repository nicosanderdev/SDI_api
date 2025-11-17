using SDI_Api.Domain.Enums;

namespace SDI_Api.Application.DTOs.Company;

public class CompanyUserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserCompanyRole Role { get; set; }
    public DateTime Created { get; set; }
}


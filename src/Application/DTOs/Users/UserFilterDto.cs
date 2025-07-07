namespace SDI_Api.Application.DTOs.Users;

public class UserFilterDto
{
    public bool? IsDeleted { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string? Role { get; set; }
    public string? SearchTerm { get; set; }
}

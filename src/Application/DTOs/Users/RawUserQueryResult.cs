namespace SDI_Api.Application.DTOs.Users;

public class RawUserQueryResult
{
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public bool Is2FAEnabled { get; set; }
    public string? Roles { get; set; }
    public long TotalCount { get; set; }
}

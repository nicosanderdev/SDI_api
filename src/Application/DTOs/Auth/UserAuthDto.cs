namespace SDI_Api.Application.DTOs.Auth;
public class UserAuthDto
{
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public bool IsAuthenticated { get; set; }
    public bool Is2FAEnabled { get; set; }
    public IEnumerable<string> Roles { get; set; } = new List<string>();
}

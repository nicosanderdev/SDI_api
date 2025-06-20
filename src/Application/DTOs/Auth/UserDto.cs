namespace SDI_Api.Web.DTOs;
public class UserDto
{
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public bool IsAuthenticated { get; set; }
    public bool Is2FAEnabled { get; set; }
    public IEnumerable<string> Roles { get; set; } = new List<string>();
}

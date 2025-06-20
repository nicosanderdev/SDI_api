namespace SDI_Api.Web.DTOs;

public class LoginResultDto
{
    public bool Succeeded { get; set; }
    public bool Requires2FA { get; set; }
    public UserDto? User { get; set; }
    public string? ErrorMessage { get; set; }
}

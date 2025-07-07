namespace SDI_Api.Application.DTOs.Auth;

public class LoginResultDto
{
    public bool Succeeded { get; set; }
    public bool Requires2FA { get; set; }
    public UserAuthDto? User { get; set; }
    public string? ErrorMessage { get; set; }
}

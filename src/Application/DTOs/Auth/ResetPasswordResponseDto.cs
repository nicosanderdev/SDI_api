namespace SDI_Api.Application.DTOs.Auth;

public class ResetPasswordResponseDto
{
    public LoginResultDto? LoginResultDto { get; set; }
    public string? ResetToken { get; set; }
}

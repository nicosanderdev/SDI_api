namespace SDI_Api.Application.DTOs.Auth;

public class ResetPasswordInitResponseDto
{
    public bool Is2FaRequired { get; set; }
    public string? Token { get; set; }
}

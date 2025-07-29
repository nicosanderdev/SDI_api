using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Application.DTOs.Auth;

public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public bool ResetEmail { get; set; } = false;
}

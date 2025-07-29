using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Application.DTOs.Auth;

public class ForgotPasswordResponseDto
{
    public bool TwoFactorEnabled { get; set; } = false;
}

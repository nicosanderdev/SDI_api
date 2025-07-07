using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Web.DTOs;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

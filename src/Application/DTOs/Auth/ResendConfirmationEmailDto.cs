using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Application.DTOs.Auth;

/// <summary>
/// DTO for requesting a new email confirmation link.
/// </summary>
public class ResendConfirmationEmailDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

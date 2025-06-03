using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Application.MemberProfile;

public class ChangePasswordDto
{
    [Required]
    public string OldPassword { get; set; } = string.Empty;
    [Required]
    public string NewPassword { get; set; } = string.Empty;
}

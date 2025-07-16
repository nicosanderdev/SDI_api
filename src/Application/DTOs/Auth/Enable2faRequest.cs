using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Application.DTOs.Auth;

public class Enable2FaRequest
{
    [Required]
    [StringLength(7, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
}

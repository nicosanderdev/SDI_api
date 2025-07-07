using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Web.DTOs;

public class Enable2faRequest
{
    [Required]
    [StringLength(7, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SDI_Api.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string? FirstName { get; set; }
    [MaxLength(100)]
    public string? LastName { get; set; }
    [MaxLength(100)]
    public string? Title { get; set; } // e.g., "Real Estate Agent", "Client"
    [MaxLength(2048)]
    public string? AvatarUrl { get; set; }
    [MaxLength(255)]
    public string? Street { get; set; }
    [MaxLength(255)]
    public string? Street2 { get; set; }
    [MaxLength(100)]
    public string? City { get; set; }
    [MaxLength(100)]
    public string? State { get; set; } // Or Province
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    [MaxLength(100)]
    public string? Country { get; set; }
}

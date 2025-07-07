using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Application.DTOs.Users;

/// <summary>
/// Data transfer object for user data returned by the API.
/// </summary>
public class UserDto
{
    [MaxLength(100)]
    public string? Id { get; set; }
    // Member
    [MaxLength(100)]
    public string? FirstName { get; set; }
    [MaxLength(100)]
    public string? LastName { get; set; }
    [MaxLength(100)]
    public string? Title { get; set; }
    [MaxLength(2048)]
    public string? AvatarUrl { get; set; }
    [MaxLength(255)]
    public string? Street { get; set; }
    [MaxLength(255)]
    public string? Street2 { get; set; }
    [MaxLength(100)]
    public string? City { get; set; }
    [MaxLength(100)]
    public string? State { get; set; }
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    [MaxLength(100)]
    public string? Country { get; set; }
    
    // User credentials
    [Required]
    public string? UserId { get; set; }
    [Required]
    [MaxLength(100)]
    public string? Email { get; set; }
    [Required]
    public bool EmailConfirmed { get; set; }
    [Required]
    [MaxLength(100)]
    public string? Password { get; set; }
    public long PhoneNumber { get; set; }
    [Required]
    public bool TwoFactorEnabled { get; set; }
}

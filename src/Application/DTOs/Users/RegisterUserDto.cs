namespace SDI_Api.Application.DTOs.Users;

/// <summary>
/// Data transfer object for registering a new user.
/// </summary>
public class RegisterUserDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}

namespace Sdi_Api.Application.DTOs.Profile;

public class ProfileDataDto
{
    public string Id { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Title { get; set; }
    public string? AvatarUrl { get; set; }
    public AddressDto? Address { get; set; }
}

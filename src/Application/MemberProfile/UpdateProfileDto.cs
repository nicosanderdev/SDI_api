using Sdi_Api.Application.DTOs.Profile;

namespace Sdi_Api.Application.MemberProfile;

public class UpdateProfileDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; } // Updating email with Identity might require confirmation flow
    public string? Phone { get; set; }
    public string? Title { get; set; }
    public AddressDto? Address { get; set; }
}

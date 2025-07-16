using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SDI_Api.Application.MemberProfile.Commands;
using SDI_Api.Application.MemberProfile.Query;

namespace SDI_Api.Web.Endpoints;

// [Authorize]
[Route("api/profile/")]
[ApiController]
public class ProfilesController : ControllerBase
{
    private readonly ISender _sender;

    public ProfilesController(ISender sender)
    {
        _sender = sender;
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUserProfile()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");
        
        var profile = await _sender.Send(new GetCurrentUserProfileQuery() { UserId = userIdValue, });
        return Ok(profile);
    }
    
    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentUserProfile([FromBody] UpdateUserProfileCommand updateUserProfileCommand)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");
        
        Guid.TryParse(userIdValue, out var userId);
        updateUserProfileCommand.UserId = userId;
        
        var updatedProfile = await _sender.Send(updateUserProfileCommand);
        return Ok(updatedProfile);
    }
    
    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadProfilePicture([FromForm] IFormFile avatar)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier); 
        Guid.TryParse(userIdValue, out var userId);
        if (avatar.Length == 0)
            throw new ArgumentException("No avatar file provided.");

        var command = new UploadProfilePictureCommand
        {
            UserId = userId,
            AvatarFile = avatar
        };
        var result = await _sender.Send(command);
        return Ok(result);
    }
}

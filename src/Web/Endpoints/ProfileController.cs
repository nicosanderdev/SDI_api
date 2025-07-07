using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sdi_Api.Application.DTOs.Profile;
using Sdi_Api.Application.MemberProfile;
using SDI_Api.Application.MemberProfile;
using SDI_Api.Application.MemberProfile.Commands;
using SDI_Api.Application.MemberProfile.Query;

namespace SDI_Api.Web.Endpoints;

[Authorize]
[Route("api/profile/")]
[ApiController]
public class ProfilesController : ControllerBase
{
    private readonly ISender _sender;

    public ProfilesController(ISender sender)
    {
        _sender = sender;
    }
    
    [HttpGet]
    public async Task<ActionResult<ProfileDataDto>> GetCurrentUserProfile()
    {
        try
        {
            var profile = await _sender.Send(new GetCurrentUserProfileQuery());
            return Ok(profile);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error fetching profile.", error = ex.Message });
        }
    }
    
    [HttpPut]
    public async Task<ActionResult<ProfileDataDto>> UpdateCurrentUserProfile([FromBody] UpdateProfileDto profileUpdateData)
    {
        try
        {
            var command = new UpdateUserProfileCommand() { ProfileUpdateData = profileUpdateData };
            var updatedProfile = await _sender.Send(command);
            return Ok(updatedProfile);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (FluentValidation.ValidationException ex)
        {
             return BadRequest(new { message = "Validation failed.", errors = ex.Errors.Select(e => new {e.PropertyName, e.ErrorMessage}) });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error updating profile.", error = ex.Message });
        }
    }
    
    // TODO - move to auth controller
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordDto passwordData)
    {
        try
        {
            var command = new ChangeUserPasswordCommand { PasswordData = passwordData };
            await _sender.Send(command);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = "Password change failed.", error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error changing password.", error = ex.Message });
        }
    }
    
    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadAvatarResponseDto>> UploadProfilePicture([FromForm] IFormFile avatarFile)
    {
        if (avatarFile.Length == 0)
        {
            return BadRequest(new { message = "No avatar file provided." });
        }
        try
        {
            var command = new UploadProfilePictureCommand { AvatarFile = avatarFile };
            var result = await _sender.Send(command);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error uploading avatar.", error = ex.Message });
        }
    }
}

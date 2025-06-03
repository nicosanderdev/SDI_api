using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sdi_Api.Application.DTOs.Profile;
using Sdi_Api.Application.MemberProfile;
using SDI_Api.Application.MemberProfile;
using SDI_Api.Application.MemberProfile.Commands;
using SDI_Api.Application.MemberProfile.Query;

namespace SDI_Api.Web.Endpoints;

[Authorize] // Ensure all actions require authentication
[Route("api/profiles")]
[ApiController]
public class ProfilesController : ControllerBase
{
    private readonly ISender _sender;

    public ProfilesController(ISender sender)
    {
        _sender = sender;
    }

    // GET api/profiles/me
    [HttpGet("me")]
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
            // Log ex
            return BadRequest(new { message = "Error fetching profile.", error = ex.Message });
        }
    }

    // PUT api/profiles/me
    [HttpPut("me")]
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
        catch (FluentValidation.ValidationException ex) // If you use FluentValidation for command DTOs
        {
             return BadRequest(new { message = "Validation failed.", errors = ex.Errors.Select(e => new {e.PropertyName, e.ErrorMessage}) });
        }
        catch (Exception ex) // General exception from UserManager update etc.
        {
            // Log ex
            return BadRequest(new { message = "Error updating profile.", error = ex.Message });
        }
    }

    // POST api/profiles/me/change-password
    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordDto passwordData)
    {
        try
        {
            var command = new ChangeUserPasswordCommand { PasswordData = passwordData };
            await _sender.Send(command);
            return NoContent(); // Or Ok("Password changed successfully");
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
         catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (FluentValidation.ValidationException ex) // Catches Identity errors repackaged as ValidationException
        {
            return BadRequest(new { message = "Password change failed.", error = ex.Message }); // Can provide more detailed errors
        }
        catch (Exception ex)
        {
            // Log ex
            return BadRequest(new { message = "Error changing password.", error = ex.Message });
        }
    }

    // POST api/profiles/me/avatar
    [HttpPost("me/avatar")]
    [Consumes("multipart/form-data")] // Important for file uploads
    public async Task<ActionResult<UploadAvatarResponseDto>> UploadProfilePicture([FromForm] IFormFile avatarFile)
    {
        if (avatarFile == null || avatarFile.Length == 0)
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
        catch (ArgumentException ex) // For file type/size validation errors
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Log ex
            return BadRequest(new { message = "Error uploading avatar.", error = ex.Message });
        }
    }
}

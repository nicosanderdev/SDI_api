using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDI_Api.Application.Company.Commands;
using SDI_Api.Application.Company.Queries;
using SDI_Api.Application.DTOs.Company;

namespace SDI_Api.Web.Endpoints;

[Authorize]
[Route("api/company/")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ISender _sender;

    public CompanyController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get current user's company
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyDto>> GetMyCompany()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");

        Guid.TryParse(userIdValue, out var userId);
        var company = await _sender.Send(new GetMyCompanyQuery { UserId = userId });
        
        if (company == null)
            return NotFound("Company not found.");

        return Ok(company);
    }

    /// <summary>
    /// Get list of users in current user's company
    /// </summary>
    [HttpGet("me/users")]
    [ProducesResponseType(typeof(List<CompanyUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CompanyUserDto>>> GetMyCompanyUsers()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");

        Guid.TryParse(userIdValue, out var userId);
        var users = await _sender.Send(new GetMyCompanyUsersQuery { UserId = userId });
        return Ok(users);
    }

    /// <summary>
    /// Add a user to the company by email
    /// </summary>
    [HttpPost("me/users")]
    [ProducesResponseType(typeof(List<CompanyUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<CompanyUserDto>>> AddUserToCompany([FromBody] AddUserToCompanyDto userData)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");

        Guid.TryParse(userIdValue, out var userId);
        var command = new AddUserToCompanyCommand
        {
            UserId = userId,
            UserData = userData
        };

        var users = await _sender.Send(command);
        return Ok(users);
    }

    /// <summary>
    /// Remove a user from the company
    /// </summary>
    [HttpDelete("me/users/{userId}")]
    [ProducesResponseType(typeof(List<CompanyUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<CompanyUserDto>>> RemoveUserFromCompany(string userId)
    {
        var currentUserIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");

        Guid.TryParse(currentUserIdValue, out var currentUserId);
        
        if (!Guid.TryParse(userId, out var userToRemoveId))
            return BadRequest("Invalid user ID format.");

        var command = new RemoveUserFromCompanyCommand
        {
            UserId = currentUserId,
            UserToRemoveId = userToRemoveId
        };

        var users = await _sender.Send(command);
        return Ok(users);
    }

    /// <summary>
    /// Update company profile
    /// </summary>
    [HttpPut("me/profile")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyDto>> UpdateCompanyProfile([FromBody] UpdateCompanyProfileDto profileData)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");

        Guid.TryParse(userIdValue, out var userId);
        var command = new UpdateCompanyProfileCommand
        {
            UserId = userId,
            ProfileData = profileData
        };

        var company = await _sender.Send(command);
        return Ok(company);
    }

    /// <summary>
    /// Upload company logo
    /// </summary>
    [HttpPost("me/logo")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadCompanyImageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadCompanyImageResponseDto>> UploadCompanyLogo([FromForm] IFormFile logo)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");

        Guid.TryParse(userIdValue, out var userId);
        
        if (logo == null || logo.Length == 0)
            return BadRequest("Logo file is required.");

        var command = new UploadCompanyLogoCommand
        {
            UserId = userId,
            LogoFile = logo
        };

        var result = await _sender.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Upload company banner
    /// </summary>
    [HttpPost("me/banner")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadCompanyImageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadCompanyImageResponseDto>> UploadCompanyBanner([FromForm] IFormFile banner)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");

        Guid.TryParse(userIdValue, out var userId);
        
        if (banner == null || banner.Length == 0)
            return BadRequest("Banner file is required.");

        var command = new UploadCompanyBannerCommand
        {
            UserId = userId,
            BannerFile = banner
        };

        var result = await _sender.Send(command);
        return Ok(result);
    }
}


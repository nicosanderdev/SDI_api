using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDI_Api.Application.DTOs.Auth;
using SDI_Api.Application.DTOs.Users;
using SDI_Api.Application.Users.Commands;
using SDI_Api.Application.Users.Queries;

namespace SDI_Api.Web.Endpoints;


[Authorize]
[Route("api/user/")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ISender _sender;
    
    public UserController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves a list of all users.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserAuthDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<UserAuthDto>>> GetUsers()
    {
        var users = await _sender.Send(new GetUsersQuery());
        return Ok(users);
    }

    /// <summary>
    /// Retrieves a specific user by their ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserAuthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserAuthDto>> GetUserById(string id)
    {
        var user = await _sender.Send(new GetUserByIdQuery { Id = id });
        return Ok(user);
    }
    
    /// <summary>
    /// Changes user password
    /// </summary>
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(UserAuthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordDto passwordData)
    {
        var command = new ChangeUserPasswordCommand { PasswordData = passwordData };
        await _sender.Send(command);
        return NoContent();
    }
    
    /// <summary>
    /// Retrieves users settings values.
    /// </summary>
    [HttpGet("settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUsersSettings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized("User is not authenticated.");
        
        var settings = await _sender.Send(new GetUserSettingsQuery(userId));
        return Ok(settings);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDI_Api.Application.DTOs.Auth;
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
    /// Registers a new user.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserAuthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserAuthDto>> RegisterUser([FromBody] RegisterUserCommand command)
    {
        try
        {
            var newUser = await _sender.Send(command);
            return Ok(newUser);
        }
        catch (FluentValidation.ValidationException ex)
        {
             return BadRequest(new { message = "Validation failed.", errors = ex.Errors.Select(e => new {e.PropertyName, e.ErrorMessage}) });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error registering user.", error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a list of all users.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserAuthDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<UserAuthDto>>> GetUsers()
    {
        try
        {
            var users = await _sender.Send(new GetUsersQuery());
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error fetching users.", error = ex.Message });
        }
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
        try
        {
            // The query would be defined in your Application layer
            var user = await _sender.Send(new GetUserByIdQuery { Id = id });
            return Ok(user);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error fetching user.", error = ex.Message });
        }
    }
}

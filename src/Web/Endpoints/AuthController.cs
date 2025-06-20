using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SDI_Api.Application.Auth.Commands;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Infrastructure.Identity;
using SDI_Api.Web.DTOs;

namespace SDI_Api.Web.Endpoints;

[ApiController]
[Route("api/auth/")]
[IgnoreAntiforgeryToken]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IIdentityService _identityService;
    
    public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ISender sender, IIdentityService identityService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _identityService = identityService;
        _sender = sender;
    }
    
    [HttpPost("loginCustom")]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        
        var result = await _sender.Send(command);

        if (result.Requires2FA)
        {
            return Ok(result);
        }
        
        if (result.Succeeded == false)
        {
            return Unauthorized(result);
        }
        
        var user = result.User!;
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id!),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!),
        };

        var roles = await _identityService.GetUserRolesAsync(user.Id!);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return Ok(result);
    }
    
    
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Ok(new { message = "Logged out successfully." });
    }
    
    [HttpGet("verify")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<IActionResult> Verify()
    {

        var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value;
        var user = await _identityService.FindUserByIdAsync(userId.ToString());
        if (user == null)
        {
            return Unauthorized(new { message = "User not authenticated." });
        }
        var roles = await _identityService.GetUserRolesAsync(user.getId()!);
    
        return Ok(new UserDto
        {
            Id = user.getId()!,
            UserName = user.getUsername()!,
            Email = user.getUserEmail()!,
            IsEmailConfirmed = user.isEmailConfirmed(),
            IsAuthenticated = true,
            Is2FAEnabled = false,
            Roles = roles
        });
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SDI_Api.Application.Auth.Commands;
using SDI_Api.Application.Auth.Queries;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.DTOs.Auth;
using SDI_Api.Application.Users.Commands;
using SDI_Api.Infrastructure.Identity;
using SendGrid.Helpers.Errors.Model;

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
    
    /// <summary>
    /// Registers a new user, and returns authentication cookie 
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserAuthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserAuthDto>> RegisterUser([FromBody] RegisterUserCommand command)
    {  
        var newUser = await _sender.Send(command);
        
        var loginCommand = new LoginCommand
        {
            UsernameOrEmail = command.RegisterUserDto.Email,
            Password = command.RegisterUserDto.Password,
            TwoFactorCode = null,
            RememberMe = false
        };

        var result = await _sender.Send(loginCommand);
        if (result.Requires2FA)
            return Ok(result);
        
        if (result.Succeeded == false)
            throw new ApplicationException();
        
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
    
    [HttpPost("login-custom")]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        
        var result = await _sender.Send(command);

        if (result.Succeeded == false)
        {
            if (result.Requires2FA)
                return Ok(result);

            throw new UnauthorizedAccessException(result.ErrorMessage ?? "Invalid login attempt.");
        }
        
        await LoginUserInfo(result);
        return Ok(result);
    }

    [HttpPost("logout-custom")]
    [Authorize]
    public async Task Logout()
    {
        if (User.Identity!.IsAuthenticated)
            await _identityService.SignOutAsync();

        foreach (var key in HttpContext.Request.Cookies.Keys)
        {
            HttpContext.Response.Cookies.Append(key, "", new CookieOptions() { Expires = DateTime.Now.AddDays(-1) });
        }
    }
    
    [HttpGet("verify")]
    [ProducesResponseType(typeof(UserAuthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<IActionResult> Verify()
    {

        var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value;
        var user = await _identityService.FindUserByIdAsync(userId.ToString());
        if (user == null)
            throw new UnauthorizedAccessException();
        
        var roles = await _identityService.GetUserRolesAsync(user.getId()!);
    
        return Ok(new UserAuthDto
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
    
    [HttpPost("forgot-password-custom")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPasswordCustom([FromBody] ForgotPasswordCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }
    
    [HttpPost("forgot-password-confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPasswordConfirmEmail([FromBody] ForgotPasswordConfirmEmailCommand command)
    {
        var result = await _sender.Send(command);
        if (result.LoginResultDto != null && result.LoginResultDto.Succeeded)
        {
            var login = await LoginUserInfo(result.LoginResultDto);
            return Ok(result.ResetToken);
        }
        return BadRequest(result.LoginResultDto!.ErrorMessage ?? "Failed to confirm email for password reset.");
    }
    
    [HttpPost("reset-password-init")]
    public async Task<IActionResult> ResetPasswordInit([FromBody] ResetPasswordInitCommand command)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        if (userEmail == null)
            throw new UnauthorizedException("User email not found in claims.");
        command.Email = userEmail;
        var result = await _sender.Send(command);
        return Ok(result);
    }
    
    [HttpPost("reset-password-2fa-validate")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword2FaValidate([FromBody] ResetPassword2FaValidateCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            throw new UnauthorizedException("User id not found in claims.");
        command.UserId = userId;
        var result = await _sender.Send(command);
        return Ok(result);
    }
    
    [HttpPost("validate-recovery-code")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateRecoveryCode([FromBody] ValidateRecoveryCodeCommand command)
    {
        // TODO : try catch?
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
            command.UserId = userId;
        
        var result = await _sender.Send(command);
        return Ok(result);
    }
    
    [HttpPost("reset-password-custom")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPasswordCustom([FromBody] ResetPasswordCommand command)
    {
        if (string.IsNullOrEmpty(command!.Email))
            command.Email = User.FindFirstValue(ClaimTypes.Email)!;
        
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpPost("confirm-email-custom")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmailCustom([FromBody] ConfirmEmailCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User ID is not provided in the request.");
        command.UserId = userId;
        await _sender.Send(command);
        return Ok();
    }
    
    [HttpGet("resend-confirmation-email-custom")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendConfirmationEmailCustom()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User ID is not provided in the request.");
        
        var command = new ResendConfirmationEmailCommand();
        command.UserId = userId;
        await _sender.Send(command);
        return Ok(new { message = "If an account with this email exists and is unconfirmed, a new verification email has been sent." });
    }

    [HttpPost("2fa/generate-custom")]
    [Authorize]
    [ProducesResponseType(typeof(GenerateTwoFactorKeyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateTwoFactorKeyCustom()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException();

        var query = new GenerateTwoFactorKeyQuery(userId);
        var result = await _sender.Send(query);

        return Ok(result);
    }

    [HttpPost("2fa/enable-2fa-first-step")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnableTwoFactorAuthCustom([FromBody] EnableTwoFactorAuthCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException();

        command.UserId = userId;
        var result = await _sender.Send(command);
        
        if (!result.Succeeded)
            throw new ArgumentException(result.ToString());

        return Ok(new { message = "Two-factor authentication has been enabled successfully." });
    }
    
    [HttpPost("2fa/enable-confirm")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteTwoFactorAuthEnabling([FromBody] CompleteTwoFactorAuthEnablingCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException();

        command.UserId = userId;
        var result = await _sender.Send(command);
        if (result == null)
            throw new ArgumentException("Failed to complete two-factor authentication enabling.");
        
        return Ok(result);
    }
    
    // Private methods //
    private async Task<IActionResult> LoginUserInfo(LoginResultDto result)
    {
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
        return Ok();
    }
}

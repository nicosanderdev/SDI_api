using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDI_Api.Application.Auth.Commands;
using SDI_Api.Application.Auth.Queries;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.DTOs.Auth;
using SDI_Api.Application.Users.Commands;
using SDI_Api.Infrastructure.Identity;
using SendGrid.Helpers.Errors.Model;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace SDI_Api.Web.Endpoints;

[ApiController]
[Route("api/auth/")]
[IgnoreAntiforgeryToken]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IIdentityService _identityService;
    private readonly IConfiguration _config;
    
    public AuthController(ISender sender, IIdentityService identityService, IConfiguration config)
    {
        _identityService = identityService;
        _sender = sender;
        _config = config;
    }
    
    /// <summary>
    /// Registers a new user, and returns authentication cookie 
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserAuthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserCommand command)
    {  
        var registerResult = await _sender.Send(command);
        if (registerResult.Succeeded == false)
        {
            return Ok(new SdiApiResponseDto()
            {
                Success = false,
                ErrorMessage = registerResult.Errors.FirstOrDefault() ?? "Registration failed. Please try again."
            });
        }
        
        var loginCommand = new LoginCommand
        {
            UsernameOrEmail = command.RegisterUserDto.Email,
            Password = command.RegisterUserDto.Password,
            TwoFactorCode = null,
            RememberMe = false
        };

        var result = await _sender.Send(loginCommand);
        if (result.Succeeded == false || result.Requires2FA)
            return Ok(new SdiApiResponseDto()
            {
                Success = false,
                ErrorMessage = result.ErrorMessage
            });
        
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
        return Ok(new SdiApiResponseDto()
        {
            Success = true,
        });
    }
    
    /// <summary>
    /// TODO
    /// </summary>
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

            return Ok(new LoginResultDto
            {
                Succeeded = false,
                Requires2FA = false,
                User = null,
                ErrorMessage = "Invalid login attempt. Please check your username and password."
            });
        }
        result.RememberMe = command.RememberMe;
        await LoginUserInfo(result);
        return Ok(result);
    }

    /// <summary>
    /// TODO
    /// </summary>
    [HttpPost("logout-custom")]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status400BadRequest)]
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
    
    /// <summary>
    /// TODO
    /// </summary>
    [HttpGet("verify")]
    [ProducesResponseType(typeof(UserAuthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<IActionResult> Verify()
    {

        var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value;
        var user = await _identityService.FindUserByIdAsync(userId.ToString());
        if (user == null)
            /* Guardrail to prevent unauthorized access */
            return Ok(new UserAuthDto
            {
                Id = null,
                UserName = null,
                Email = null,
                IsEmailConfirmed = false,
                IsAuthenticated = false,
                Is2FAEnabled = false,
                Roles = new List<string>()
            });
        
        var roles = await _identityService.GetUserRolesAsync(user.getId()!);
    
        return Ok(new UserAuthDto
        {
            Id = user.getId()!,
            UserName = user.getUsername()!,
            Email = user.getUserEmail()!,
            IsEmailConfirmed = user.isEmailConfirmed(),
            IsAuthenticated = true,
            Is2FAEnabled = user.isTwoFactorEnabled(),
            Roles = roles
        });
    }
    
    /// <summary>
    /// TODO
    /// </summary>
    [HttpPost("forgot-password-custom")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPasswordCustom([FromBody] ForgotPasswordCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }
    
    /// <summary>
    /// TODO
    /// </summary>
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
    
    /// <summary>
    /// TODO
    /// </summary>
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
    
    /// <summary>
    /// TODO
    /// </summary>
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
    
    /// <summary>
    /// TODO
    /// </summary>
    [HttpPost("validate-recovery-code")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateRecoveryCode([FromBody] ValidateRecoveryCodeCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
            command.UserId = userId;
        
        var result = await _sender.Send(command);
        return Ok(result);
    }
    
    /// <summary>
    /// TODO
    /// </summary>
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

    /// <summary>
    /// TODO
    /// </summary>
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
    
    /// <summary>
    /// TODO
    /// </summary>
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

    /// <summary>
    /// TODO
    /// </summary>
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

    /// <summary>
    /// TODO
    /// </summary>
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
            return Ok(new SdiApiResponseDto()
            {
                Success = false,
                ErrorMessage = "Failed to enable two-factor authentication. Please try again."
            });

        return Ok(new SdiApiResponseDto()
        {
            Success = true,
        });
    }
    
    /// <summary>
    /// TODO
    /// </summary>
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
    
    /// //////////////////////////////////
    /// GOOGLE AUTHENTICATION ENDPOINTS //
    /// //////////////////////////////////
    
    /// <summary>
    /// TODO
    /// </summary>
    [HttpGet("google-login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GoogleLogin()
    {
        var redirectUrl = _config["Authentication:Google:RedirectUrl"];
        if (string.IsNullOrEmpty(redirectUrl))
            throw new ApplicationException("Google redirect url is not configured");
        
        var properties = _identityService.ConfigureExternalAuthenticationProperties("Google", redirectUrl).Result;
        return Task.FromResult<IActionResult>(new ChallengeResult("Google", properties));
    }

    /// <summary>
    /// TODO
    /// </summary>
    [HttpGet("google-callback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GoogleCallback()
    {
        var info = await _identityService.GetExternalLoginInfoAsync();
        if (info == null)
            return Ok(new LoginResultDto
            {
                Succeeded = false,
                ErrorMessage = "User's Google information not available"
            });
        
        var result = _identityService.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey);
        ApplicationUser? user;

        if (result.IsCompletedSuccessfully)
            user = (ApplicationUser?)await _identityService.FindLoginAsync(info.LoginProvider, info.ProviderKey);
        else
        {
            // User does not exist or is not linked. Create a new user.
            var email = info.Principal.FindFirstValue(ClaimTypes.Email)!;
            user = (ApplicationUser?)(await _identityService.FindUserByEmailAsync(email));
            if (user == null)
            {
                // Create a new user if no one has this email
                user = new ApplicationUser() { UserName = email, Email = email, EmailConfirmed = true };
                var createUserResult = 
                    await _identityService.CreateUserAsync(user.getUserEmail()!, null, user.getUsername(), null, CancellationToken.None);
                
                if (!createUserResult.Result.Succeeded)
                    return Ok(new LoginResultDto
                    {
                        Succeeded = false,
                        ErrorMessage = "Error creating user"
                    });
            }
            
            SignInResult addLoginResult = await _identityService.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
                return Ok(new LoginResultDto
                {
                    Succeeded = false,
                    ErrorMessage = "Error logging in user"
                });
        }

        var userAuth = new UserAuthDto(user!);
        return await LoginUserInfo(new LoginResultDto
        {
            Succeeded = true,
            Requires2FA = false,
            User = userAuth,
            ErrorMessage = null,
            RememberMe = false
        });
    }

    /// /////////////////
    /// Private methods//
    /// /////////////////
    
    /// <summary>
    /// TODO
    /// </summary>
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
        var authProperties = new AuthenticationProperties { IsPersistent = result.RememberMe };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
        return Ok();
    }
}

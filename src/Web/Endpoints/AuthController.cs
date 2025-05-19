using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Dtos;
using SDI_Api.Infrastructure.Identity;

namespace SDI_Api.Web.Endpoints;

[Route("api/auth/")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    
    public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ISender sender)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _sender = sender;
    }

    [HttpGet("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return Unauthorized("Invalid login request");
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, request.Username)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return Ok(new { message = "Login successful" });
    }

    [HttpGet("signout")]
    public IActionResult Logout()
    {
        return SignOut(new AuthenticationProperties
        {
            RedirectUri = "/"
        }, CookieAuthenticationDefaults.AuthenticationScheme);
    }
}

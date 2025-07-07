using Microsoft.AspNetCore.Identity;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Web.DTOs;

namespace SDI_Api.Application.Auth.Commands;

public class LoginCommand : IRequest<LoginResultDto>
{
    public string? UsernameOrEmail { get; set; } 
    public string? Password { get; set; }
    public string? TwoFactorCode { get; set; }
    public bool RememberMe { get; set; }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateProvider _emailTemplateProvider;

    public LoginCommandHandler(IIdentityService identityService, IMapper mapper, IEmailService emailService, IEmailTemplateProvider emailTemplateProvider)
    {
        _identityService = identityService;
        _mapper = mapper;
        _emailService = emailService;
        _emailTemplateProvider = emailTemplateProvider;
    }

    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByEmailAsync(request.UsernameOrEmail!);
        if (user == null && request.UsernameOrEmail!.Contains("@"))
        {
            user = await _identityService.FindUserByUsernameAsync(request.UsernameOrEmail);
        }

        if (user == null)
        {
            return new LoginResultDto
            {
                Succeeded = false, 
                ErrorMessage = "Invalid credentials."
            };
        }
        
        // --- 2FA Flow ---
        // If a 2FA code is provided, attempt 2FA sign-in directly.
        if (!string.IsNullOrWhiteSpace(request.TwoFactorCode))
        {
            var twoFactorResult = await _identityService.TwoFactorAuthenticatorSignInAsync(request.TwoFactorCode, request.RememberMe, rememberClient: false);
            var user2FA = await _identityService.GetTwoFactorAuthenticationUserAsync();
            if (user2FA == null)
            {
                return new LoginResultDto
                {
                    Succeeded = false, 
                    ErrorMessage = "Invalid 2FA code."
                };
            }
            return await CreateLoginResultDto(twoFactorResult, user2FA);
        }

        //TODO : Review mapping and initialization of properties
        var userAuth = _mapper.Map<IUser>(user);
        
        var checkPasswordResult = await _identityService.CheckPasswordSignInAsync(userAuth, request.Password!, lockoutOnFailure: true);
        return await CreateLoginResultDto(checkPasswordResult, userAuth);
    }
    
    private async Task<LoginResultDto> CreateLoginResultDto(SignInResult result, IUser user)
    {
        if (result.RequiresTwoFactor)
        {
            return new LoginResultDto
            {
                Succeeded = false, 
                Requires2FA = true
            };
        }
        if (result.Succeeded)
        {
            var roles = await _identityService.GetUserRolesAsync(user.getId()!);
            return new LoginResultDto
            {
                Succeeded = true,
                User = new UserDto
                {
                    Id = user.getId(),
                    UserName = user.getUsername(),
                    Email = user.getUserEmail(),
                    IsEmailConfirmed = user.isEmailConfirmed(),
                    IsAuthenticated = true,
                    Is2FAEnabled = await _identityService.GetTwoFactorEnabledAsync(user),
                    Roles = roles
                }
            };
        }

        if (result.IsLockedOut)
        {
            return new LoginResultDto
            {
                Succeeded = false,
                ErrorMessage = "This account has been locked out due to too many failed login attempts."
            };
        }
        if (result.IsNotAllowed)
        {
             return new LoginResultDto
             {
                 Succeeded = false, 
                 ErrorMessage = "Login is not allowed. Please confirm your email address."
             };
        }
        // Generic failure for incorrect password or other issues.
        return new LoginResultDto
        {
            Succeeded = false, 
            ErrorMessage = "Invalid credentials."
        };
    }
}

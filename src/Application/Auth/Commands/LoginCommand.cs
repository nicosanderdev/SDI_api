﻿using Microsoft.AspNetCore.Identity;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.DTOs.Auth;

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
            user = await _identityService.FindUserByUsernameAsync(request.UsernameOrEmail);

        if (user == null)
            return new LoginResultDto { Succeeded = false, ErrorMessage = "Invalid credentials." };
        
        // --- 2FA Flow ---
        // If a 2FA code is provided, attempt 2FA sign-in directly.
        if (user.isTwoFactorEnabled())
        {
            if (!string.IsNullOrWhiteSpace(request.TwoFactorCode))
            {
                var twoFactorResult = await _identityService.TwoFactorAuthenticatorSignInAsync(user.getId()!, request.TwoFactorCode);
                if (!twoFactorResult.Succeeded)
                    return await CreateLoginResultDto(SignInResult.Failed, user);
                
                return await CreateLoginResultDto(SignInResult.Success, user);
            }
            
            var result2fa = await _identityService.CheckPasswordSignInAsync(user, request.Password!, lockoutOnFailure: true);
            if (result2fa.Succeeded || result2fa.RequiresTwoFactor)
            {
                var verificationCode = await _identityService.GenerateTwoFactorAuthenticatorKeyAsync(user);
                var emailBody = _emailTemplateProvider.GetTwoFactorCodeBody(verificationCode.sharedKey);
                await _emailService.SendEmailAsync(user.getUserEmail()!, "Two-Factor Authentication Code", emailBody);
                return await CreateLoginResultDto(SignInResult.TwoFactorRequired, null);
            }
        }
        var checkPasswordResult = await _identityService.CheckPasswordSignInAsync(user, request.Password!, lockoutOnFailure: true);
        return await CreateLoginResultDto(checkPasswordResult, user);
        
    }
    
    private async Task<LoginResultDto> CreateLoginResultDto(SignInResult result, IUser? user)
    {
        if (result.RequiresTwoFactor)
            return new LoginResultDto() { Succeeded = false, Requires2FA = true };
        
        if (result.Succeeded)
        {
            var roles = await _identityService.GetUserRolesAsync(user!.getId()!);
            return new LoginResultDto
            {
                Succeeded = true,
                User = new UserAuthDto
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
            return new LoginResultDto { Succeeded = false, ErrorMessage = "This account has been locked out due to too many failed login attempts." };
        
        if (result.IsNotAllowed)
             return new LoginResultDto { Succeeded = false, ErrorMessage = "Login is not allowed. Please confirm your email address." };
        
        return new LoginResultDto { Succeeded = false, ErrorMessage = "Invalid credentials." };
    }
}

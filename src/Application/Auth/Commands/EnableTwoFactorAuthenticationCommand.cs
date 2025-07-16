using System.ComponentModel.DataAnnotations;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;

namespace SDI_Api.Application.Auth.Commands;

/// <summary>
/// Command to enable two-factor authentication for a user by verifying their authenticator code.
/// </summary>
public record EnableTwoFactorAuthCommand : IRequest<Result>
{
    public string? UserId { get; set; }
    [Required]
    public string? Password { get; set; }
}

/// <summary>
/// Handles the logic for the EnableTwoFactorAuthCommand.
/// </summary>
public class EnableTwoFactorAuthCommandHandler : IRequestHandler<EnableTwoFactorAuthCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateProvider _emailTemplateProvider;

    public EnableTwoFactorAuthCommandHandler(IIdentityService identityService, IEmailService emailService, IEmailTemplateProvider emailTemplateProvider)
    {
        _identityService = identityService;
        _emailService = emailService;
        _emailTemplateProvider = emailTemplateProvider;
    }

    public async Task<Result> Handle(EnableTwoFactorAuthCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(request.UserId!);
        if (user == null)
            throw new UnauthorizedAccessException();
        
        if (!user.isEmailConfirmed())
            throw new ArgumentException();
        
        var signInResult = await _identityService.CheckPasswordSignInAsync(user, request.Password!, false);
        if (!signInResult.Succeeded)
            throw new UnauthorizedAccessException();
            
        var verificationCode = await _identityService.GenerateTwoFactorAuthenticatorKeyAsync(user);
        var emailBody = _emailTemplateProvider.GetTwoFactorCodeBody(verificationCode.sharedKey);
        await _emailService.SendEmailAsync(user.getUserEmail()!, "Two-Factor Authentication Code", emailBody);
        
        return Result.Success();
    }
}

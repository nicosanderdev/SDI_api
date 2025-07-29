using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Auth;

namespace SDI_Api.Application.Auth.Commands;

public record ResetPasswordInitCommand : IRequest<ResetPasswordInitResponseDto>
{
    public string? Email { get; set; }
}

public class ResetPasswordInitCommandHandler : IRequestHandler<ResetPasswordInitCommand, ResetPasswordInitResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateProvider _emailTemplateProvider;

    public ResetPasswordInitCommandHandler(IIdentityService identityService, IEmailService emailService, IEmailTemplateProvider emailTemplateProvider)
    {
        _identityService = identityService;
        _emailService = emailService;
        _emailTemplateProvider = emailTemplateProvider;
    }

    public async Task<ResetPasswordInitResponseDto> Handle(ResetPasswordInitCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByEmailAsync(request.Email!);
        if (user == null)
            return new ResetPasswordInitResponseDto() { Is2FaRequired = false }; // Error

        if (user.isTwoFactorEnabled())
        { 
            var verificationCode = await _identityService.GenerateTwoFactorAuthenticatorKeyAsync(user);
            var emailBody = _emailTemplateProvider.GetTwoFactorCodeBody(verificationCode.sharedKey);
            await _emailService.SendEmailAsync(user.getUserEmail()!, "Two-Factor Authentication Code", emailBody);
            return new ResetPasswordInitResponseDto() { Is2FaRequired = true }; // Continue with 2fa authentication
        }
        
        var token = await _identityService.GeneratePasswordResetTokenAsync(user);
        if (string.IsNullOrEmpty(token))
            return new ResetPasswordInitResponseDto() { Is2FaRequired = false }; // Error
        
        return new ResetPasswordInitResponseDto() { Is2FaRequired = false, Token = token }; // Continue with reset password
    }
}

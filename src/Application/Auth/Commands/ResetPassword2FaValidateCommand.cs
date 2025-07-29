using System.Security.Authentication;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Auth;

namespace SDI_Api.Application.Auth.Commands;

public record ResetPassword2FaValidateCommand : IRequest<ResetPasswordInitResponseDto>
{
    public string? UserId { get; set; }
    public string? TwoFactorCode { get; set; }
}

public class ResetPassword2FaValidateCommandHanlder : IRequestHandler<ResetPassword2FaValidateCommand, ResetPasswordInitResponseDto>
{
    private readonly IIdentityService _identityService;
    
    public ResetPassword2FaValidateCommandHanlder(IIdentityService identityService, IEmailConfirmationTokenService confirmationTokenService)
    {
        _identityService = identityService;
    }

    public async Task<ResetPasswordInitResponseDto> Handle(ResetPassword2FaValidateCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(request.UserId!);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");
        
        var result = await _identityService.TwoFactorAuthenticatorSignInAsync(user.getId()!, request.TwoFactorCode!);
        if (!result.Succeeded)
            throw new AuthenticationException();

        var token = await _identityService.GeneratePasswordResetTokenAsync(user);
        return new ResetPasswordInitResponseDto() { Is2FaRequired = false, Token = token };
    }
}

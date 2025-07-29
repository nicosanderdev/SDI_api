using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.DTOs.Auth;

namespace SDI_Api.Application.Auth.Commands;

public record ForgotPasswordConfirmEmailCommand : IRequest<ResetPasswordResponseDto> 
{
    public string? UserEmail { get; set; }
    public string? EmailToken { get; set; }
}

public class ForgotPasswordConfirmEmailCommandHandler : IRequestHandler<ForgotPasswordConfirmEmailCommand, ResetPasswordResponseDto>
{
    private IEmailConfirmationTokenService _confirmationTokenService;
    private IIdentityService _identityService;
    
    public ForgotPasswordConfirmEmailCommandHandler(IEmailConfirmationTokenService confirmationTokenService, IIdentityService identityService)
    {
        _confirmationTokenService = confirmationTokenService;
        _identityService = identityService;
    }

    public async Task<ResetPasswordResponseDto> Handle(ForgotPasswordConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByEmailAsync(request.UserEmail!);
        if (user == null)
            throw new UnauthorizedAccessException("User not found."); 
        
        var result = _confirmationTokenService.ValidateToken(request.EmailToken!, request.UserEmail!);
        if (!result.IsValid)
            throw new UnauthorizedAccessException("Invalid email confirmation token.");

        var loginResult = new LoginResultDto()
        {
            Succeeded = result.IsValid,
            Requires2FA = false,
            User = new UserAuthDto(user),
        };

        return new ResetPasswordResponseDto()
        {
            LoginResultDto = loginResult, ResetToken = await _identityService.GeneratePasswordResetTokenAsync(user)
        };
    }
}

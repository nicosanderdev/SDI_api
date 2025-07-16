using System.Security.Authentication;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.DTOs.Auth;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Auth.Commands;

public record CompleteTwoFactorAuthEnablingCommand : IRequest<RecoveryCodeResponseDto>
{
    public string? UserId { get; set; }
    public string? TwoFactorCode { get; set; }
}

public class CompleteTwoFactorAuthEnablingCommandHandler : IRequestHandler<CompleteTwoFactorAuthEnablingCommand, RecoveryCodeResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IRecoveryCodeGenerator _recoveryCodeGenerator;
    private readonly IApplicationDbContext _applicationDbContext;
    
    public CompleteTwoFactorAuthEnablingCommandHandler(IIdentityService identityService, IRecoveryCodeGenerator recoveryCodeGenerator, IApplicationDbContext applicationDbContext)
    {
        _identityService = identityService;
        _recoveryCodeGenerator = recoveryCodeGenerator;
        _applicationDbContext = applicationDbContext;
    }
    
    public async Task<RecoveryCodeResponseDto> Handle(CompleteTwoFactorAuthEnablingCommand command, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(command.UserId!);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");
        
        var result = await _identityService.TwoFactorAuthenticatorSignInAsync(user.getId()!, command.TwoFactorCode!);
        if (!result.Succeeded)
            throw new AuthenticationException();

        await _identityService.EnableTwoFactorAuthenticationAsync(command.UserId!);
        
        var code = _recoveryCodeGenerator.GenerateCode();
        Guid.TryParse(command.UserId!, out var userIdGuid);
        var recoveryCode = new RecoveryCode()
        {
            UserId = userIdGuid,
            Code = code,
            CreatedAt = DateTime.UtcNow
        };
        _applicationDbContext.RecoveryCodes.Add(recoveryCode);
        await _applicationDbContext.SaveChangesAsync(cancellationToken);
        
        return new RecoveryCodeResponseDto
        {
            RecoveryCode = code
        };
    }
}

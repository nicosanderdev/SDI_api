using Microsoft.AspNetCore.Mvc.ApplicationModels;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;

namespace SDI_Api.Application.Auth.Commands;

public record ValidateRecoveryCodeCommand : IRequest<Result>
{
    public string? UserId { get; set; }
    public string? RecoveryCode { get; set; }
}

public class
    ForgotPasswordValidateRecoveryCodeCommandHandler : IRequestHandler<ValidateRecoveryCodeCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    
    public ForgotPasswordValidateRecoveryCodeCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _context = context;
    }

    public async Task<Result> Handle(ValidateRecoveryCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(request.UserId!);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");
        
        var recoveryCode = await _context.RecoveryCodes.Where(rc => rc.UserId == Guid.Parse(request.UserId!)) 
            .FirstOrDefaultAsync(rc => rc.Code == request.RecoveryCode, cancellationToken);

        if (recoveryCode == null)
            return Result.Failure(new List<string>() { "Invalid recovery code." });
        
        if (recoveryCode.UsedAt < DateTime.Now)
            return Result.Failure(new List<string>() { "Recovery code has already been used." });
        
        recoveryCode.UsedAt = DateTime.Now;
        return Result.Success();
    }
}

using MediatR;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;

namespace SDI_Api.Application.Auth.Commands;

/// <summary>
/// Command to enable two-factor authentication for a user by verifying their authenticator code.
/// </summary>
public record EnableTwoFactorAuthCommand(string UserId, string VerificationCode) : IRequest<Result>;

/// <summary>
/// Handles the logic for the EnableTwoFactorAuthCommand.
/// </summary>
public class EnableTwoFactorAuthCommandHandler : IRequestHandler<EnableTwoFactorAuthCommand, Result>
{
    private readonly IIdentityService _identityService;

    public EnableTwoFactorAuthCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(EnableTwoFactorAuthCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(request.UserId);

        if (user is null)
        {
            return Result.Failure(new[] { "User not found." });
        }
        var result = await _identityService.EnableTwoFactorAuthenticationAsync(user.getId()!);
        return result;
    }
}

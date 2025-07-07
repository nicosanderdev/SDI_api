using MediatR;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;

namespace SDI_Api.Application.Auth.Commands;

/// <summary>
/// Command to reset a user's password using a token.
/// This record is also used as the DTO for the API request body.
/// </summary>
public record ResetPasswordCommand : IRequest<Result>
{
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

/// <summary>
/// Handles the logic for the ResetPasswordCommand.
/// </summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByEmailAsync(request.Email);
        if (user is null)
            return Result.Failure(new[] { "Invalid token or email." });

        var result = await _identityService.ResetPasswordAsync(user.getId()!, request.Token, request.NewPassword);
        return result;
    }
}

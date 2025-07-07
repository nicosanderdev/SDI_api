using MediatR;
using SDI_Api.Application.Common.Exceptions;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;

namespace SDI_Api.Application.Auth.Commands;

/// <summary>
/// Command to confirm a user's email address using a user ID and token.
/// This record is also used as the DTO for the API request body.
/// </summary>
public record ConfirmEmailCommand : IRequest<Result>
{
    public string UserId { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
}

/// <summary>
/// Handles the logic for the ConfirmEmailCommand.
/// </summary>
public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result>
{
    private readonly IIdentityService _identityService;

    public ConfirmEmailCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException("User", request.UserId);

        var result = await _identityService.ConfirmEmailAsync(user.getId()!, request.Token);
        return result;
    }
}

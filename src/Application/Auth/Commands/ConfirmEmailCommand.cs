using FluentValidation.Results;
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
    public string? UserId { get; set; }
    public string? Token { get; set; }
    public string? Email { get; set; }
}

/// <summary>
/// Handles the logic for the ConfirmEmailCommand.
/// </summary>
public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailConfirmationTokenService _tokenService;

    public ConfirmEmailCommandHandler(IIdentityService identityService, IEmailConfirmationTokenService tokenService)
    {
        _identityService = identityService;
        _tokenService = tokenService;
    }

    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(request.UserId!);
        if (user == null)
            throw new NotFoundException("User", request.UserId!);
        
        var tokenValidationResult = _tokenService.ValidateToken(request.Email!, request.Token!);
        if (!tokenValidationResult.IsValid)
            throw new ArgumentException("Token is not valid");
        
        var result = await _identityService.ConfirmEmailAsync(user.getId()!);
        return result;
    }
}

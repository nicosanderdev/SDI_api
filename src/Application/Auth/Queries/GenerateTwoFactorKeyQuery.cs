using MediatR;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Auth;

namespace SDI_Api.Application.Auth.Queries;

/// <summary>
/// Query to generate a new two-factor authenticator key for a user.
/// </summary>
/// <param name="UserId">The ID of the user for whom to generate the key.</param>
public record GenerateTwoFactorKeyQuery(string UserId) : IRequest<GenerateTwoFactorKeyResponse>;

/// <summary>
/// Handles the logic for the GenerateTwoFactorKeyQuery.
/// </summary>
public class GenerateTwoFactorKeyQueryHandler : IRequestHandler<GenerateTwoFactorKeyQuery, GenerateTwoFactorKeyResponse>
{
    private readonly IIdentityService _identityService;

    public GenerateTwoFactorKeyQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<GenerateTwoFactorKeyResponse> Handle(GenerateTwoFactorKeyQuery request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(request.UserId);
        if (user is null)
            throw new UnauthorizedAccessException();
        
        var code = await _identityService.GenerateTwoFactorAuthenticatorKeyAsync(user);
        return new GenerateTwoFactorKeyResponse()
        {
            SharedKey = code.sharedKey, 
            AuthenticatorUri = code.authenticatorUri
        };
    }
}

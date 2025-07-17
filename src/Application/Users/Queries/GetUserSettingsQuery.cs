using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Users;

namespace SDI_Api.Application.Users.Queries;

public record GetUserSettingsQuery(string? UserId) : IRequest<UserSettingsDto>;

public class GetUserSettingsQueryHandler : IRequestHandler<GetUserSettingsQuery, UserSettingsDto>
{
    private readonly IIdentityService _identityService;

    public GetUserSettingsQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<UserSettingsDto> Handle(GetUserSettingsQuery request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(request.UserId!);
        if (user == null)
            throw new ArgumentException("User not found");

        return new UserSettingsDto(user.isEmailConfirmed(), user.isTwoFactorEnabled());
    }
}

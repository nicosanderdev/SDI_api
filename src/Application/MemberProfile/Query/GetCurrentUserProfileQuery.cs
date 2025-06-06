using SDI_Api.Application.Common.Interfaces;
using Sdi_Api.Application.DTOs.Profile;

namespace SDI_Api.Application.MemberProfile.Query;

public class GetCurrentUserProfileQuery : IRequest<ProfileDataDto> { }

public class GetCurrentUserProfileQueryHandler : IRequestHandler<GetCurrentUserProfileQuery, ProfileDataDto>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetCurrentUserProfileQueryHandler(IIdentityService identityService, ICurrentUserService currentUserService, IMapper mapper)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<ProfileDataDto> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        if (userId != Guid.Empty)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var user = await _identityService.FindUserByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException(nameof(IUser), userId.ToString());
        }

        return _mapper.Map<ProfileDataDto>(user);
    }
}

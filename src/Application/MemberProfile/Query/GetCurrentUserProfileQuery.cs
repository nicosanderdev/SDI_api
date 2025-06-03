using Sdi_Api.Application.DTOs.Profile;

namespace SDI_Api.Application.MemberProfile.Query;

public class GetCurrentUserProfileQuery : IRequest<ProfileDataDto> { }

public class GetCurrentUserProfileQueryHandler : IRequestHandler<GetCurrentUserProfileQuery, ProfileDataDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetCurrentUserProfileQueryHandler(
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<ProfileDataDto> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserIdGuid();
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
        {
            throw new NotFoundException(nameof(ApplicationUser), userId.Value);
        }

        return _mapper.Map<ProfileDataDto>(user);
    }
}

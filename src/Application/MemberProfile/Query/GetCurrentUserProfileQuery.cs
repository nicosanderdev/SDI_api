using SDI_Api.Application.Common.Interfaces;
using Sdi_Api.Application.DTOs.Profile;

namespace SDI_Api.Application.MemberProfile.Query;

public class GetCurrentUserProfileQuery : IRequest<ProfileDataDto>
{
    public string? UserId { get; set; }
}

public class GetCurrentUserProfileQueryHandler : IRequestHandler<GetCurrentUserProfileQuery, ProfileDataDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public GetCurrentUserProfileQueryHandler(IApplicationDbContext context, IIdentityService identityService, IMapper mapper)
    {
        _context = context;
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<ProfileDataDto> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
    {
        if (String.IsNullOrEmpty(request.UserId))
            throw new UnauthorizedAccessException("User is not authenticated.");
        
        var user = await _identityService.FindUserByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException(nameof(IUser), request.UserId);

        Guid.TryParse(request.UserId, out var userIdGuid);
        var member = await _context.Members
            .Where(x => x.UserId == userIdGuid).FirstOrDefaultAsync(cancellationToken);

        var profileDataDto = _mapper.Map<ProfileDataDto>(user);
        return _mapper.Map(member, profileDataDto);
    }
}

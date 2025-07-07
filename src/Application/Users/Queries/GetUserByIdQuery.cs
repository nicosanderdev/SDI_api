using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Users;

namespace SDI_Api.Application.Users.Queries;

public class GetUserByIdQuery : IRequest<UserDto?>
{
    public required string Id;
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public GetUserByIdQueryHandler(IMapper mapper, IApplicationDbContext context, IIdentityService identityService)
    {
        _mapper = mapper;
        _context = context;
        _identityService = identityService;
    }
    
    // TODO: create validator for command
    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(request.Id);
        if (user == null)
            return null;

        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId.ToString() == request.Id, cancellationToken);
        if (member == null)
            return null;
        
        var userDto = _mapper.Map<UserDto>(user);
        // Merge both entities info 
        _mapper.Map(member, userDto);
        return userDto;
    }
}

using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.DTOs.Users;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Users.Queries;

public class GetUsersQuery : IRequest<PaginatedResult<UserDto>>
{
    public int PageNumber { get; set; } 
    public int PageSize { get; set; }
    public UserFilterDto FilterDto { get; set; } = new();
}

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedResult<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;

    public GetUsersQueryHandler(IApplicationDbContext context, IMapper mapper, IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _identityService = identityService;
    }
    
    public async Task<PaginatedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Member> query = _context.Members.AsNoTracking();

        var filter = request.FilterDto;
        
        if (filter.IsDeleted.HasValue)
            query = query.Where(x => x.IsDeleted == filter.IsDeleted.Value);

        if (filter.CreatedAfter.HasValue)
            query = query.Where(x => x.Created >= filter.CreatedAfter.Value);

        if (filter.CreatedBefore.HasValue)
            query = query.Where(x => x.Created <= filter.CreatedBefore.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(x => 
                (x.FirstName != null && x.FirstName.ToLower().Contains(searchTerm)) ||
                (x.LastName != null && x.LastName.ToLower().Contains(searchTerm))
            );
        }
        
        var members = await query.ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var memberIds = members.Select(m => m.Id!);
        var users = await _identityService.FindUsersByIdListAsync(memberIds.ToList());
        
        var userDtos = _mapper.Map<List<UserDto>>(users);
        
        return await PaginatedResult<UserDto>.CreateAsync(userDtos.AsQueryable(), request.PageNumber, request.PageSize);
    }
}

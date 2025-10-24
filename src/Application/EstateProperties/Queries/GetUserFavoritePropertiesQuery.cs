using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.EstateProperties;

namespace SDI_Api.Application.EstateProperties.Queries;

public class GetUserFavoritePropertiesQuery : IRequest<List<Guid>>
{
    public Guid UserId { get; set; }    
}

public class
    GetUserFavoritePropertiesQueryHandler : IRequestHandler<GetUserFavoritePropertiesQuery,
    List<Guid>>
{
    IApplicationDbContext _context;
    
    public GetUserFavoritePropertiesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
    }
    
    public async Task<List<Guid>> Handle(GetUserFavoritePropertiesQuery request, CancellationToken cancellationToken)
    {
        var memberId = await _context.Members
            .AsNoTracking()
            .Where(m => m.UserId == request.UserId)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return await _context.Favorites
            .AsNoTracking()
            .Where(f => f.MemberId == memberId)
            .Select(f => f.EstatePropertyId)
            .ToListAsync(cancellationToken);
    }
}

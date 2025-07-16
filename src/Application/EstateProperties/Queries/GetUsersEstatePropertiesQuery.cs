using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.DTOs.EstateProperties;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.EstateProperties.Queries;

public class GetUsersEstatePropertiesQuery : IRequest<PaginatedResult<UsersEstatePropertyDto>>
{
    public Guid UserId { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class GetUsersEstatePropertiesQueryHandler : IRequestHandler<GetUsersEstatePropertiesQuery, PaginatedResult<UsersEstatePropertyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUsersEstatePropertiesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<UsersEstatePropertyDto>> Handle(GetUsersEstatePropertiesQuery request, CancellationToken cancellationToken)
    {

        IQueryable<EstateProperty> query = _context.EstateProperties
            .Where(ep => ep.Owner.UserId == request.UserId && !ep.IsDeleted)
            .Include(ep => ep.PropertyImages)
            .Include(ep => ep.EstatePropertyValues)
            .AsNoTracking();

        // MapperProfile filters EstatePropertyValues and PropertyImages
        query = query.OrderByDescending(p => p.CreatedOnUtc);
        var result = await PaginatedResult<EstateProperty>.CreateAsync(query, request.PageNumber, request.PageSize);
        var estatePropertyDtos = _mapper.Map<List<UsersEstatePropertyDto>>(result.Items);
        return new PaginatedResult<UsersEstatePropertyDto>(estatePropertyDtos, result.TotalCount, result.PageNumber, result.TotalPages);
    }
}

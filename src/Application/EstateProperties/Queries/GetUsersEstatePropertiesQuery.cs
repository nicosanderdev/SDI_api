using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.DTOs.EstateProperties;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.EstateProperties.Queries;

public class GetUsersEstatePropertiesQuery : IRequest<PaginatedResult<UsersEstatePropertyDto>>
{
    public Guid? UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
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
        query = query.OrderByDescending(p => p.Created);
        var result = await PaginatedResult<EstateProperty>.CreateAsync(query, request.PageNumber, request.PageSize);
        List<UsersEstatePropertyDto> estatePropertyDtos = new List<UsersEstatePropertyDto>();
        foreach (var estateProperty in result.Items)
        {
            var dto = _mapper.Map<UsersEstatePropertyDto>(estateProperty);
            _mapper.Map(estateProperty.EstatePropertyValues.FirstOrDefault(ep => ep.IsFeatured)!, dto);
            _mapper.Map(estateProperty.PropertyImages.Where(pi => !pi.IsDeleted), dto.Images);
            estatePropertyDtos.Add(dto);
        }
        return new PaginatedResult<UsersEstatePropertyDto>(estatePropertyDtos, result.TotalCount, result.PageNumber, result.TotalPages);
    }
}

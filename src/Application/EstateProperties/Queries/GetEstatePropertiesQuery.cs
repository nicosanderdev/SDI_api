using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using Sdi_Api.Application.Dtos;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.EstateProperties.Queries;

public record GetEstatePropertiesQuery(int PageNumber, int PageSize) : IRequest<PaginatedResult<EstatePropertyDto>>;

public class GetEstatePropertiesQueryHandler : IRequestHandler<GetEstatePropertiesQuery, PaginatedResult<EstatePropertyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    
    public GetEstatePropertiesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<PaginatedResult<EstatePropertyDto>> Handle(GetEstatePropertiesQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EstateProperty> query = _context.EstateProperties
            .Include(p => p.MainImage)
            .Include(p => p.PropertyImages)
            .Include(p => p.FeaturedDescription)
            .Include(p => p.EstatePropertyDescriptions)
            .AsNoTracking();

        // Example filtering (add more as needed)
        // if (!string.IsNullOrEmpty(request.StatusFilter) && Enum.TryParse<PropertyStatus>(request.StatusFilter, true, out var status))
        // {
        //     query = query.Where(p => p.Status == status);
        // }
            
        // Example sorting (add more as needed)
        query = query.OrderByDescending(p => p.CreatedOnUtc);

        // ProjectTo<EstatePropertyDto> handles the mapping including derived fields like MainImageUrl via AutoMapper profile
        return await PaginatedResult<EstatePropertyDto>.CreateAsync(
            query.ProjectTo<EstatePropertyDto>(_mapper.ConfigurationProvider), 
            request.PageNumber,
            request.PageSize);
    }
}

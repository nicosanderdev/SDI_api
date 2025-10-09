using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.DTOs.EstateProperties;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Enums;

namespace SDI_Api.Application.EstateProperties.Queries;

public record GetEstatePropertiesQuery : IRequest<PaginatedResult<PublicEstatePropertyDto>>
{
    public int PageNumber { get; set; } 
    public int PageSize { get; set; }
    public PropertyFilterDto Filter { get; set; } = new();
}

public class GetEstatePropertiesQueryHandler : IRequestHandler<GetEstatePropertiesQuery, PaginatedResult<PublicEstatePropertyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    
    public GetEstatePropertiesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<PaginatedResult<PublicEstatePropertyDto>> Handle(GetEstatePropertiesQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EstateProperty> query = _context.EstateProperties
            .Include(p => p.PropertyImages.Where(pi => pi.IsMain))
            .Where(p => p.EstatePropertyValues.FirstOrDefault(epv => epv.IsFeatured)!.IsPropertyVisible)
            .AsNoTracking();

        var filter = request.Filter;
        
        if (filter.IsDeleted.HasValue)
            query = query.Where(p => p.IsDeleted == filter.IsDeleted.Value);
            
        if (!string.IsNullOrEmpty(filter.OwnerId))
            query = query.Where(p => p.OwnerId.ToString() == filter.OwnerId);
        
        if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<PropertyStatus>(filter.Status, true, out var statusEnum))
            query = query.Where(p => p.EstatePropertyValues.FirstOrDefault(epv => epv.IsFeatured)!.Status == statusEnum);

        if (filter.CreatedAfter.HasValue)
            query = query.Where(p => p.Created >= filter.CreatedAfter.Value);

        if (filter.CreatedBefore.HasValue)
            query = query.Where(p => p.Created <= filter.CreatedBefore.Value);
        
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower().Trim();
            query = query.Where(p => 
                p.Title.ToLower().Contains(term) ||
                (p.StreetName != null && p.StreetName.ToLower().Contains(term)) ||
                (p.City != null && p.City.ToLower().Contains(term))
            );
        }
        
        if (filter.SwLat.HasValue && filter.SwLng.HasValue && filter.NeLat.HasValue && filter.NeLng.HasValue)
        {
            var swLat = (decimal)filter.SwLat.Value;
            var swLng = (decimal)filter.SwLng.Value;
            var neLat = (decimal)filter.NeLat.Value;
            var neLng = (decimal)filter.NeLng.Value;
            
            query = query.Where(p => p.LocationLatitude >= swLat && p.LocationLatitude <= neLat);
            
            if (swLng <= neLng)
            {
                query = query.Where(p => p.LocationLongitude >= swLng && p.LocationLongitude <= neLng);
            }
            else
            {
                query = query.Where(p => p.LocationLongitude >= swLng || p.LocationLongitude <= neLng);
            }
        }
        
        query = query.OrderByDescending(p => p.Created);
        var result = await PaginatedResult<EstateProperty>.CreateAsync(query, request.PageNumber, request.PageSize);
        var estatePropertyDtos = _mapper.Map<List<PublicEstatePropertyDto>>(result.Items);
        return new PaginatedResult<PublicEstatePropertyDto>(estatePropertyDtos, result.TotalCount, result.PageNumber, result.TotalPages);
    }
}

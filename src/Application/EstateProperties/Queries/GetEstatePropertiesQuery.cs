using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.DTOs.EstateProperties;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Enums;

namespace SDI_Api.Application.EstateProperties.Queries;

public record GetEstatePropertiesQuery : IRequest<PaginatedResult<PublicEstatePropertyDto>>
{
    public List<Guid>? Ids { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
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
        IQueryable<EstateProperty> baseQuery = _context.EstateProperties
            .Where(p => p.EstatePropertyValues.FirstOrDefault(epv => epv.IsFeatured)!.IsPropertyVisible)
            .Where(p => !p.IsDeleted) 
            .AsNoTracking();
        
        var queryByFilter = baseQuery;
        
        if (request.Filter?.IncludeImages == true)
            baseQuery = baseQuery.Include(ep => ep.PropertyImages);

        if (request.Filter?.IncludeVideos == true)
            baseQuery = baseQuery.Include(ep => ep.PropertyVideos);
        
        if (request.Filter?.IncludeAmenities == true)
            baseQuery = baseQuery.Include(ep => ep.EstatePropertyAmenities)
                .ThenInclude(epa => epa.Amenity);
        
        if (request.Ids is { Count: > 0 })
        {
            var queryByIds = baseQuery.Where(p => request.Ids.Contains(p.Id));
            var items = await queryByIds.ToListAsync(cancellationToken);
            var dtos = _mapper.Map<List<PublicEstatePropertyDto>>(items);
            
            return new PaginatedResult<PublicEstatePropertyDto>(dtos, dtos.Count, 1, 1);
        }
    
        var filter = request.Filter;
        if (!string.IsNullOrEmpty(filter!.OwnerId) && Guid.TryParse(filter.OwnerId, out var ownerGuid))
            queryByFilter = queryByFilter.Where(p => p.OwnerId == ownerGuid);

        if (!string.IsNullOrEmpty(filter.Status) &&
            Enum.TryParse<PropertyStatus>(filter.Status, true, out var statusEnum))
            queryByFilter = queryByFilter.Where(p =>
                p.EstatePropertyValues.FirstOrDefault(epv => epv.IsFeatured)!.Status == statusEnum);

        if (filter.CreatedAfter.HasValue)
            queryByFilter = queryByFilter.Where(p => p.Created >= filter.CreatedAfter.Value);

        if (filter.CreatedBefore.HasValue)
            queryByFilter = queryByFilter.Where(p => p.Created <= filter.CreatedBefore.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower().Trim();
            queryByFilter = queryByFilter.Where(p =>
                p.Title.ToLower().Contains(term) ||
                (p.StreetName != null && p.StreetName.ToLower().Contains(term)) ||
                (p.City != null && p.City.ToLower().Contains(term))
            );
        }

        queryByFilter = queryByFilter.OrderByDescending(p => p.Created);
        
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

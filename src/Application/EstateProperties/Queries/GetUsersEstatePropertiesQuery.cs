using System.Text.Json.Serialization;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.Dtos;
using SDI_Api.Application.DTOs.EstateProperties;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Enums;

namespace SDI_Api.Application.EstateProperties.Queries;

public class GetUsersEstatePropertiesQuery : IRequest<PaginatedResult<UsersEstatePropertyDto>>
{
    // This will be set by the controller from the user's claims.
    // It should not be bindable from the query string for security.
    [JsonIgnore]
    public Guid UserId { get; set; }

    // To fetch specific properties by their IDs.
    // If this list has values, filters are ignored.
    public List<Guid>? Ids { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // This filter now includes the 'IsDeleted' flag.
    public PropertyFilterDto Filter { get; set; } = new();
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
        IQueryable<EstateProperty> baseQuery = _context.EstateProperties
            .Where(ep => ep.Owner.UserId == request.UserId)
            .Include(ep => ep.EstatePropertyValues)
            .AsNoTracking();
        
        var queryByFilter = baseQuery;
        
        var filter = request.Filter;
        
        if (request.Filter?.IncludeImages == true)
            baseQuery = baseQuery.Include(ep => ep.PropertyImages);

        if (request.Filter?.IncludeDocuments == true)
            baseQuery = baseQuery.Include(ep => ep.PropertyDocuments);

        if (request.Filter?.IncludeVideos == true)
            baseQuery = baseQuery.Include(ep => ep.PropertyVideos);
        
        if (request.Filter?.IncludeAmenities == true)
            baseQuery = baseQuery.Include(ep => ep.EstatePropertyAmenities)
                .ThenInclude(epa => epa.Amenity);
        
        if (request.Ids is { Count: > 0 })
        {
            var queryByIds = baseQuery.Where(p => request.Ids.Contains(p.Id));
            var items = await queryByIds.ToListAsync(cancellationToken);
            var dtos = MapEntitiesToDtos(items);

            return new PaginatedResult<UsersEstatePropertyDto>(dtos, dtos.Count, 1, 1);
        }

        queryByFilter = baseQuery;
        queryByFilter = queryByFilter.Where(p => p.IsDeleted == filter.IsDeleted);
        
        if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<PropertyStatus>(filter.Status, true, out var statusEnum))
            queryByFilter = queryByFilter.Where(p => 
                p.EstatePropertyValues.FirstOrDefault(epv => epv.IsFeatured)!.Status == statusEnum);
        
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower().Trim();
            queryByFilter = queryByFilter.Where(p => p.Title.ToLower().Contains(term));
        }
        
        queryByFilter = queryByFilter.OrderByDescending(p => p.Created);
        var paginatedResult = await PaginatedResult<EstateProperty>.CreateAsync(queryByFilter, request.PageNumber, request.PageSize);
        var estatePropertyDtos = MapEntitiesToDtos(paginatedResult.Items);

        return new PaginatedResult<UsersEstatePropertyDto>(estatePropertyDtos, paginatedResult.TotalCount, paginatedResult.PageNumber, paginatedResult.TotalPages);
    }

    /// <summary>
    /// Helper method to encapsulate the complex mapping logic and avoid duplication.
    /// </summary>
    private List<UsersEstatePropertyDto> MapEntitiesToDtos(List<EstateProperty> properties)
    {
        var dtos = new List<UsersEstatePropertyDto>();
        foreach (var property in properties)
        {
            var dto = _mapper.Map<UsersEstatePropertyDto>(property);
            var featuredValue = property.EstatePropertyValues.FirstOrDefault(epv => epv.IsFeatured);
            if (featuredValue != null)
                _mapper.Map(featuredValue, dto);
            
            var images = property.PropertyImages.Where(pi => !pi.IsDeleted).ToList();
            dto.PropertyImages = _mapper.Map<List<PropertyImageDto>>(images);
            dtos.Add(dto);
        }
        return dtos;
    }
}

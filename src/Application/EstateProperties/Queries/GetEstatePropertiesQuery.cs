using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using Sdi_Api.Application.Dtos;
using SDI_Api.Application.DTOs.EstateProperties;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Enums;

namespace SDI_Api.Application.EstateProperties.Queries;

public record GetEstatePropertiesQuery : IRequest<PaginatedResult<EstatePropertyDto>>
{
    public int PageNumber { get; set; } 
    public int PageSize { get; set; }
    public PropertyFilterDto Filter { get; set; } = new();
}

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

        // === START OF NEW FILTERING LOGIC ===

        var filter = request.Filter;
        
        if (filter.IsDeleted.HasValue)
        {
            query = query.Where(p => p.IsDeleted == filter.IsDeleted.Value);
        }
        
        if (!string.IsNullOrEmpty(filter.OwnerId))
        {
            query = query.Where(p => p.OwnerId.ToString() == filter.OwnerId);
        }
        
        if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<PropertyStatus>(filter.Status, true, out var statusEnum))
        {
            query = query.Where(p => p.Status == statusEnum);
        }
        
        if (filter.CreatedAfter.HasValue)
        {
            query = query.Where(p => p.CreatedOnUtc >= filter.CreatedAfter.Value);
        }
        if (filter.CreatedBefore.HasValue)
        {
            query = query.Where(p => p.CreatedOnUtc <= filter.CreatedBefore.Value);
        }
        
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower().Trim();
            query = query.Where(p => 
                p.Title.ToLower().Contains(term) ||
                (p.Address != null && p.Address.ToLower().Contains(term)) ||
                (p.City != null && p.City.ToLower().Contains(term))
            );
        }
            
        
        query = query.OrderByDescending(p => p.CreatedOnUtc);
        var result = await PaginatedResult<EstateProperty>.CreateAsync(query, request.PageNumber, request.PageSize);
        var estatePropertyDtos = _mapper.Map<List<EstatePropertyDto>>(result.Items);
        return new PaginatedResult<EstatePropertyDto>(estatePropertyDtos, result.TotalCount, result.PageNumber, result.TotalPages);
    }
}

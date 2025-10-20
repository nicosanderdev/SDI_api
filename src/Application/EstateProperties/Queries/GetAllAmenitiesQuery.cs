using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.EstateProperties;

namespace SDI_Api.Application.EstateProperties.Queries;

public record GetAllAmenitiesQuery : IRequest<List<AmenityDto>> {}

public class GetAllAmenitiesQueryHandler : IRequestHandler<GetAllAmenitiesQuery, List<AmenityDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllAmenitiesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<List<AmenityDto>> Handle(GetAllAmenitiesQuery request, CancellationToken cancellationToken)
    {
        var amenities = await _context.Amenities.Where(a => !a.IsDeleted).ToListAsync();
        return _mapper.Map<List<AmenityDto>>(amenities);
    }
}

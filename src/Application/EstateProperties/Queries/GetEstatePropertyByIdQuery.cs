using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.EstateProperties;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.EstateProperties.Queries;

public sealed record GetEstatePropertyByIdQuery(Guid id) : IRequest<PublicEstatePropertyDto>;

public class GetEstatePropertyByIdQueryHandler : IRequestHandler<GetEstatePropertyByIdQuery, PublicEstatePropertyDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEstatePropertyByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PublicEstatePropertyDto> Handle(GetEstatePropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.EstateProperties
            .Include(p => p.PropertyImages.Where(pi => pi.IsMain))
            .Include(p => p.PropertyImages)
            .Include(p => p.EstatePropertyValues)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(EstateProperty), request.id.ToString());
        }

        return _mapper.Map<PublicEstatePropertyDto>(entity);
    }
}

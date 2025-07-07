using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;
using YourProject.Dto.Properties;

namespace SDI_Api.Application.EstateProperties.Queries;

public sealed record GetEstatePropertyByIdQuery(Guid id) : IRequest<EstatePropertyDto>;

public class GetEstatePropertyByIdQueryHandler : IRequestHandler<GetEstatePropertyByIdQuery, EstatePropertyDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEstatePropertyByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<EstatePropertyDto> Handle(GetEstatePropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.EstateProperties
            .Include(p => p.MainImage)
            .Include(p => p.PropertyImages)
            .Include(p => p.FeaturedValues)
            .Include(p => p.EstatePropertyValues)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(EstateProperty), request.id.ToString());
        }

        return _mapper.Map<EstatePropertyDto>(entity);
    }
}

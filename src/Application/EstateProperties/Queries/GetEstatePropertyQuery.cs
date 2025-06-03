using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.EstateProperties.Queries;

public sealed record GetEstatePropertyQuery(Guid estatePropertyId) : IRequest<EstateProperty>;

public class GetEstatePropertyQueryHandler : IRequestHandler<GetEstatePropertyQuery, EstateProperty>
{
    private readonly IApplicationDbContext _context;

    public GetEstatePropertyQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<EstateProperty> Handle(GetEstatePropertyQuery request, CancellationToken cancellationToken)
    {
        return await _context.EstateProperties.Where(ep =>
                ep.IsPublic && !ep.IsDeleted && ep.Id == request.estatePropertyId)
            .Include(ep => ep.FeaturedDescription)
            .Include(ep => ep.MainImage)
            .Include(ep => ep.PropertyImages)
            /* .Include(ep => ep.MessageThreads)!
                .ThenInclude(mt => mt.Messages)*/
            .FirstAsync(cancellationToken);
    }
}

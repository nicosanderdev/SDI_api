using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.EstateProperties.Queries;

public record GetEstatePropertiesQuery() : IRequest<IReadOnlyCollection<EstateProperty>>;

public class GetEstatePropertiesQueryHandler : IRequestHandler<GetEstatePropertiesQuery, IReadOnlyCollection<EstateProperty>>
{
    private readonly IApplicationDbContext _context;
    
    public GetEstatePropertiesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IReadOnlyCollection<EstateProperty>> Handle(GetEstatePropertiesQuery request, CancellationToken cancellationToken)
    {
        return await _context.EstateProperties.ToListAsync(cancellationToken);
    }
}

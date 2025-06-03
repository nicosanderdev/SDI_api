using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.EstateProperties.Queries;

public sealed record GetUsersEstatePropertiesQuery(Guid userId) : IRequest<IReadOnlyCollection<EstateProperty>>;

public class GetUsersEstatePropertiesQueryHandler : IRequestHandler<GetUsersEstatePropertiesQuery, IReadOnlyCollection<EstateProperty>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersEstatePropertiesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<IReadOnlyCollection<EstateProperty>> Handle(GetUsersEstatePropertiesQuery command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("");
        /* return await _context.EstateProperties
            .Where(ep => !ep.IsDeleted && ep.UserId == command.userId)
            .ToListAsync(cancellationToken); */
    }
}

using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Subscriptions;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Subscriptions.Queries;

public class GetPlansQuery : IRequest<List<PlanDto>>
{
}

public class GetPlansQueryHandler : IRequestHandler<GetPlansQuery, List<PlanDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPlansQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<PlanDto>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _context.Plans
            .Where(p => p.IsActive)
            .OrderBy(p => p.MonthlyPrice)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<PlanDto>>(plans);
    }
}


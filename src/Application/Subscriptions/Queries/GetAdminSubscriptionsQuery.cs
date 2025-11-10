using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.DTOs.Subscriptions;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Subscriptions.Queries;

public class GetAdminSubscriptionsQuery : IRequest<PaginatedResult<SubscriptionDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetAdminSubscriptionsQueryHandler : IRequestHandler<GetAdminSubscriptionsQuery, PaginatedResult<SubscriptionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAdminSubscriptionsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<SubscriptionDto>> Handle(GetAdminSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Subscriptions
            .Include(s => s.Plan)
            .OrderByDescending(s => s.Created)
            .AsNoTracking();

        var result = await PaginatedResult<Subscription>.CreateAsync(query, request.PageNumber, request.PageSize);
        var dtos = _mapper.Map<List<SubscriptionDto>>(result.Items);
        
        return new PaginatedResult<SubscriptionDto>(dtos, result.TotalCount, result.PageNumber, result.TotalPages);
    }
}


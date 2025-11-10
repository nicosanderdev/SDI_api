using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.DTOs.Subscriptions;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Enums;

namespace SDI_Api.Application.Subscriptions.Queries;

public class GetBillingHistoryQuery : IRequest<PaginatedResult<BillingHistoryDto>>
{
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetBillingHistoryQueryHandler : IRequestHandler<GetBillingHistoryQuery, PaginatedResult<BillingHistoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBillingHistoryQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<BillingHistoryDto>> Handle(GetBillingHistoryQuery request, CancellationToken cancellationToken)
    {
        // Get user's subscription or company subscription
        var subscription = await GetUserSubscription(request.UserId, cancellationToken);
        
        if (subscription == null)
        {
            return new PaginatedResult<BillingHistoryDto>(new List<BillingHistoryDto>(), 0, request.PageNumber, request.PageSize);
        }

        var query = _context.BillingHistories
            .Where(bh => bh.SubscriptionId == subscription.Id)
            .OrderByDescending(bh => bh.Created)
            .AsNoTracking();

        var result = await PaginatedResult<BillingHistory>.CreateAsync(query, request.PageNumber, request.PageSize);
        var dtos = _mapper.Map<List<BillingHistoryDto>>(result.Items);
        
        return new PaginatedResult<BillingHistoryDto>(dtos, result.TotalCount, result.PageNumber, result.TotalPages);
    }

    private async Task<Subscription?> GetUserSubscription(Guid userId, CancellationToken cancellationToken)
    {
        // Try user subscription first
        var userSubscription = await _context.Subscriptions
            .Where(s => s.OwnerType == OwnerType.User && s.OwnerId == userId)
            .OrderByDescending(s => s.Created)
            .FirstOrDefaultAsync(cancellationToken);

        if (userSubscription != null)
            return userSubscription;

        // Check company subscription
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);

        if (member == null)
            return null;

        var userCompany = await _context.UserCompanies
            .Include(uc => uc.Company)
            .Where(uc => uc.MemberId == member.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (userCompany == null)
            return null;

        return await _context.Subscriptions
            .Where(s => s.OwnerType == OwnerType.Company && s.OwnerId == userCompany.CompanyId)
            .OrderByDescending(s => s.Created)
            .FirstOrDefaultAsync(cancellationToken);
    }
}


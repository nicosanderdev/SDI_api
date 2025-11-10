using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Subscriptions;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Enums;

namespace SDI_Api.Application.Subscriptions.Queries;

public class GetCurrentSubscriptionQuery : IRequest<SubscriptionDto?>
{
    public Guid UserId { get; set; }
}

public class GetCurrentSubscriptionQueryHandler : IRequestHandler<GetCurrentSubscriptionQuery, SubscriptionDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCurrentSubscriptionQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<SubscriptionDto?> Handle(GetCurrentSubscriptionQuery request, CancellationToken cancellationToken)
    {
        // First try to find user's personal subscription
        var userSubscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.OwnerType == OwnerType.User && s.OwnerId == request.UserId)
            .OrderByDescending(s => s.Created)
            .FirstOrDefaultAsync(cancellationToken);

        if (userSubscription != null)
        {
            return _mapper.Map<SubscriptionDto>(userSubscription);
        }

        // If no personal subscription, check if user belongs to a company with a subscription
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.UserId == request.UserId, cancellationToken);

        if (member == null)
            return null;

        var userCompany = await _context.UserCompanies
            .Include(uc => uc.Company)
            .Where(uc => uc.MemberId == member.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (userCompany == null)
            return null;

        var companySubscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.OwnerType == OwnerType.Company && s.OwnerId == userCompany.CompanyId)
            .OrderByDescending(s => s.Created)
            .FirstOrDefaultAsync(cancellationToken);

        return companySubscription != null ? _mapper.Map<SubscriptionDto>(companySubscription) : null;
    }
}


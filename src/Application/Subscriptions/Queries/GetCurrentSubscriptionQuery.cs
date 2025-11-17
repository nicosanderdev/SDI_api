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
        var memberId = await _context.Members.Where(m => m.UserId.Equals(request.UserId))
            .Select(m => m.Id).FirstOrDefaultAsync(cancellationToken); 
        
        var userSubscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.OwnerType == OwnerType.User && s.OwnerId.Equals(memberId))
            .OrderByDescending(s => s.Created)
            .FirstOrDefaultAsync(cancellationToken);

        if (userSubscription != null)
            return _mapper.Map<SubscriptionDto>(userSubscription);
        
        var userCompany = await _context.UserCompanies.Where(uc => uc.MemberId.Equals(memberId))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (userCompany == null)
            return null;

        var companyId = await _context.Companies.Where(c => c.Id.Equals(userCompany.CompanyId))
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (companyId == Guid.Empty)
            return null;

        var companySubscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.OwnerType == OwnerType.Company && s.OwnerId.Equals(companyId))
            .OrderByDescending(s => s.Created)
            .FirstOrDefaultAsync(cancellationToken);

        return companySubscription != null ? _mapper.Map<SubscriptionDto>(companySubscription) : null;
    }
}

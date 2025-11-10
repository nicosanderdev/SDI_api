using SDI_Api.Application.Common.Exceptions;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Subscriptions;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Enums;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;

namespace SDI_Api.Application.Subscriptions.Commands;

public class ChangePlanCommand : IRequest<SubscriptionDto>
{
    public Guid UserId { get; set; }
    public ChangePlanRequestDto Request { get; set; } = null!;
}

public class ChangePlanCommandHandler : IRequestHandler<ChangePlanCommand, SubscriptionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ChangePlanCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<SubscriptionDto> Handle(ChangePlanCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Request.PlanId, out var planId))
        {
            throw new ArgumentException("Invalid plan ID format.");
        }

        var plan = await _context.Plans.FindAsync(new object[] { planId }, cancellationToken);
        if (plan == null || !plan.IsActive)
        {
            throw new NotFoundException(nameof(Plan), request.Request.PlanId);
        }

        // Get user's current subscription
        var subscription = await GetUserSubscription(request.UserId, cancellationToken);
        
        if (subscription == null)
        {
            throw new NotFoundException(nameof(Subscription), "No active subscription found.");
        }

        // Verify user has permission to change this subscription
        if (subscription.OwnerType == OwnerType.Company)
        {
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == request.UserId, cancellationToken);
            
            if (member == null)
                throw new NotFoundException(nameof(Member), request.UserId.ToString());

            var userCompany = await _context.UserCompanies
                .Where(uc => uc.MemberId == member.Id && uc.CompanyId == subscription.OwnerId)
                .FirstOrDefaultAsync(cancellationToken);

            if (userCompany == null || (userCompany.Role != UserCompanyRole.owner && userCompany.Role != UserCompanyRole.admin))
            {
                throw new ForbiddenAccessException();
            }
        }
        else if (subscription.OwnerId != request.UserId)
        {
            throw new ForbiddenAccessException();
        }

        // Update subscription
        subscription.PlanId = planId;
        subscription.Plan = plan;
        subscription.UpdatedAt = DateTime.UtcNow;

        // TODO: If proration is enabled, calculate prorated amount and create invoice
        // TODO: Integrate with payment provider to update subscription

        await _context.SaveChangesAsync(cancellationToken);

        // Reload with plan
        await _context.Subscriptions.Where(s => s.Id == subscription.Id)
            .Include(s => s.Plan)
            .LoadAsync(cancellationToken);

        return _mapper.Map<SubscriptionDto>(subscription);
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


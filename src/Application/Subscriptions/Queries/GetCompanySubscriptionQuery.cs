using SDI_Api.Application.Common.Exceptions;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Security;
using SDI_Api.Application.DTOs.Subscriptions;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Enums;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;

namespace SDI_Api.Application.Subscriptions.Queries;

[Authorize]
public class GetCompanySubscriptionQuery : IRequest<SubscriptionDto?>
{
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
}

public class GetCompanySubscriptionQueryHandler : IRequestHandler<GetCompanySubscriptionQuery, SubscriptionDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCompanySubscriptionQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<SubscriptionDto?> Handle(GetCompanySubscriptionQuery request, CancellationToken cancellationToken)
    {
        // Verify user has access to this company (owner or admin)
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.UserId == request.UserId, cancellationToken);

        if (member == null)
            throw new NotFoundException(nameof(Member), request.UserId.ToString());

        var userCompany = await _context.UserCompanies
            .Where(uc => uc.MemberId == member.Id && uc.CompanyId == request.CompanyId)
            .FirstOrDefaultAsync(cancellationToken);

        if (userCompany == null || (userCompany.Role != UserCompanyRole.owner && userCompany.Role != UserCompanyRole.admin))
        {
            throw new ForbiddenAccessException();
        }

        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.OwnerType == OwnerType.Company && s.OwnerId == request.CompanyId)
            .OrderByDescending(s => s.Created)
            .FirstOrDefaultAsync(cancellationToken);

        return subscription != null ? _mapper.Map<SubscriptionDto>(subscription) : null;
    }
}


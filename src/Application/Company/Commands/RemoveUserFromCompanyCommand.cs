using SDI_Api.Application.Common.Exceptions;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Company;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Enums;
using NotFoundException = SDI_Api.Application.Common.Exceptions.NotFoundException;

namespace SDI_Api.Application.Company.Commands;

public class RemoveUserFromCompanyCommand : IRequest<List<CompanyUserDto>>
{
    public Guid UserId { get; set; }
    public Guid UserToRemoveId { get; set; }
}

public class RemoveUserFromCompanyCommandHandler : IRequestHandler<RemoveUserFromCompanyCommand, List<CompanyUserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public RemoveUserFromCompanyCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IMapper mapper)
    {
        _context = context;
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<List<CompanyUserDto>> Handle(RemoveUserFromCompanyCommand request, CancellationToken cancellationToken)
    {
        // Get current user's member
        var currentMember = await _context.Members
            .Where(m => m.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentMember == null)
            throw new NotFoundException(nameof(Member), request.UserId.ToString());

        // Get current user's company
        var currentUserCompany = await _context.UserCompanies
            .Where(uc => uc.MemberId == currentMember.Id)
            .Include(uc => uc.Company)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentUserCompany == null)
            throw new NotFoundException("Company", "User is not associated with a company.");

        // Get member to remove
        var memberToRemove = await _context.Members
            .Where(m => m.UserId == request.UserToRemoveId)
            .FirstOrDefaultAsync(cancellationToken);

        if (memberToRemove == null)
            throw new NotFoundException(nameof(Member), request.UserToRemoveId.ToString());

        // Get user company relationship to remove
        var userCompanyToRemove = await _context.UserCompanies
            .Where(uc => uc.MemberId == memberToRemove.Id && uc.CompanyId == currentUserCompany.CompanyId)
            .FirstOrDefaultAsync(cancellationToken);

        if (userCompanyToRemove == null)
            throw new NotFoundException("UserCompany", "User is not a member of this company.");

        // Prevent removing the owner
        if (userCompanyToRemove.Role == UserCompanyRole.owner)
            throw new InvalidOperationException("Cannot remove the company owner.");

        // Check if current user is owner or admin
        if (currentUserCompany.Role != UserCompanyRole.owner && currentUserCompany.Role != UserCompanyRole.admin)
            throw new UnauthorizedAccessException("Only owners and admins can remove users from the company.");

        // Remove user from company
        _context.UserCompanies.Remove(userCompanyToRemove);
        await _context.SaveChangesAsync(cancellationToken);

        // Return updated list of users
        var getUsersQuery = new Queries.GetMyCompanyUsersQuery { UserId = request.UserId };
        var handler = new Queries.GetMyCompanyUsersQueryHandler(_context, _identityService, _mapper);
        return await handler.Handle(getUsersQuery, cancellationToken);
    }
}


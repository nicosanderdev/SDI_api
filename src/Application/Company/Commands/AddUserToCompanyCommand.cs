using SDI_Api.Application.Common.Exceptions;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Company;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Enums;
using NotFoundException = SDI_Api.Application.Common.Exceptions.NotFoundException;

namespace SDI_Api.Application.Company.Commands;

public class AddUserToCompanyCommand : IRequest<List<CompanyUserDto>>
{
    public Guid UserId { get; set; }
    public required AddUserToCompanyDto UserData { get; set; }
}

public class AddUserToCompanyCommandHandler : IRequestHandler<AddUserToCompanyCommand, List<CompanyUserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public AddUserToCompanyCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IMapper mapper)
    {
        _context = context;
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<List<CompanyUserDto>> Handle(AddUserToCompanyCommand request, CancellationToken cancellationToken)
    {
        // Get current user's member
        var currentMember = await _context.Members
            .Where(m => m.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentMember == null)
            throw new NotFoundException(nameof(Member), request.UserId.ToString());

        // Get current user's company
        var userCompany = await _context.UserCompanies
            .Where(uc => uc.MemberId == currentMember.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (userCompany == null)
            throw new NotFoundException("Company", "User is not associated with a company.");

        // Find user by email
        var user = await _identityService.FindUserByEmailAsync(request.UserData.Email);
        if (user == null)
            throw new NotFoundException("User", $"User with email {request.UserData.Email} not found.");

        var userIdGuid = Guid.Parse(user.getId()!);
        
        // Get member for the user
        var memberToAdd = await _context.Members
            .Where(m => m.UserId == userIdGuid)
            .FirstOrDefaultAsync(cancellationToken);

        if (memberToAdd == null)
            throw new NotFoundException(nameof(Member), user.getId()!);

        // Check if user is already in the company
        var existingUserCompany = await _context.UserCompanies
            .Where(uc => uc.MemberId == memberToAdd.Id && uc.CompanyId == userCompany.CompanyId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingUserCompany != null)
            throw new InvalidOperationException("User is already a member of this company.");

        // TODO: Check subscription limits if applicable
        // For now, we'll skip this check as subscription limit logic would need to be implemented

        // Add user to company with default role of manager
        var newUserCompany = new UserCompany
        {
            MemberId = memberToAdd.Id,
            CompanyId = userCompany.CompanyId,
            Role = UserCompanyRole.manager
        };

        _context.UserCompanies.Add(newUserCompany);
        await _context.SaveChangesAsync(cancellationToken);

        // Return updated list of users
        var getUsersQuery = new Queries.GetMyCompanyUsersQuery { UserId = request.UserId };
        var handler = new Queries.GetMyCompanyUsersQueryHandler(_context, _identityService, _mapper);
        return await handler.Handle(getUsersQuery, cancellationToken);
    }
}


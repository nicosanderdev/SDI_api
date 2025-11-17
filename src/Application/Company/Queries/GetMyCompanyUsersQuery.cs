using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Company;

namespace SDI_Api.Application.Company.Queries;

public class GetMyCompanyUsersQuery : IRequest<List<CompanyUserDto>>
{
    public Guid UserId { get; set; }
}

public class GetMyCompanyUsersQueryHandler : IRequestHandler<GetMyCompanyUsersQuery, List<CompanyUserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public GetMyCompanyUsersQueryHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IMapper mapper)
    {
        _context = context;
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<List<CompanyUserDto>> Handle(GetMyCompanyUsersQuery request, CancellationToken cancellationToken)
    {
        var member = await _context.Members
            .Where(m => m.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (member == null)
            return new List<CompanyUserDto>();

        var userCompany = await _context.UserCompanies
            .Where(uc => uc.MemberId == member.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (userCompany == null)
            return new List<CompanyUserDto>();

        var companyUsers = await _context.UserCompanies
            .Where(uc => uc.CompanyId == userCompany.CompanyId)
            .Include(uc => uc.Company)
            .ToListAsync(cancellationToken);

        var memberIds = companyUsers.Select(uc => uc.MemberId).ToList();
        var members = await _context.Members
            .Where(m => memberIds.Contains(m.Id))
            .ToListAsync(cancellationToken);

        var result = new List<CompanyUserDto>();
        foreach (var userCompanyEntity in companyUsers)
        {
            var memberEntity = members.FirstOrDefault(m => m.Id == userCompanyEntity.MemberId);
            if (memberEntity == null)
                continue;

            var user = await _identityService.FindUserByIdAsync(memberEntity.UserId.ToString());
            result.Add(new CompanyUserDto
            {
                Id = userCompanyEntity.Id.ToString(),
                UserId = memberEntity.UserId.ToString(),
                Email = user?.getUserEmail() ?? string.Empty,
                FirstName = memberEntity.FirstName,
                LastName = memberEntity.LastName,
                Role = userCompanyEntity.Role,
                Created = DateTime.UtcNow
            });
        }

        return result;
    }
}


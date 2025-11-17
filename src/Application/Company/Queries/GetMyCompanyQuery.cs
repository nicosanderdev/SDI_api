using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Company;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Enums;

namespace SDI_Api.Application.Company.Queries;

public class GetMyCompanyQuery : IRequest<CompanyDto?>
{
    public Guid UserId { get; set; }
}

public class GetMyCompanyQueryHandler : IRequestHandler<GetMyCompanyQuery, CompanyDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public GetMyCompanyQueryHandler(IApplicationDbContext context, IIdentityService identityService, IMapper mapper)
    {
        _context = context;
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<CompanyDto?> Handle(GetMyCompanyQuery request, CancellationToken cancellationToken)
    {
        var memberDb = await _context.Members
            .Where(m => m.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (memberDb == null)
            return null;

        var userCompany = await _context.UserCompanies
            .Where(uc => uc.MemberId == memberDb.Id)
            .Include(uc => uc.Company)
                .ThenInclude(c => c.UserCompanies)
            .FirstOrDefaultAsync(cancellationToken);

        if (userCompany == null)
            return null;

        var company = userCompany.Company;
        var companyDto = _mapper.Map<CompanyDto>(company);

        // Map users if needed
        if (company.UserCompanies != null && company.UserCompanies.Any())
        {
            var memberIds = company.UserCompanies.Select(uc => uc.MemberId).ToList();
            var members = await _context.Members
                .Where(m => memberIds.Contains(m.Id))
                .ToListAsync(cancellationToken);

            var usersList = new List<CompanyUserDto>();
            foreach (var uc in company.UserCompanies)
            {
                var member = members.FirstOrDefault(m => m.Id == uc.MemberId);
                if (member == null)
                    continue;

                var user = await _identityService.FindUserByIdAsync(member.UserId.ToString());
                var companyUserDto = _mapper.Map<CompanyUserDto>(uc);
                companyUserDto.Email = user?.getUserEmail() ?? string.Empty;
                companyUserDto.Created = uc.Company.CreatedAt;
                usersList.Add(companyUserDto);
            }
            companyDto.Users = usersList;
        }

        return companyDto;
    }
}


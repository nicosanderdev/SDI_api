using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Exceptions;

namespace SDI_Api.Application.Common.Security;

public class CompanyAccessService : ICompanyAccessService
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public CompanyAccessService(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task ValidateCompanyAccessAsync(Guid userId, string? companyId)
    {
        if (string.IsNullOrEmpty(companyId))
            return;

        var memberId = await _context.Members.Where(m => m.UserId == userId)
            .Select(m => m.Id).FirstOrDefaultAsync();
        
        switch (companyId.ToLower())
        {
            case "all":
                break;

            case "all-companies":
                var userCompanies = await _context.UserCompanies
                    .Where(uc => uc.MemberId == memberId)
                    .Select(uc => uc.CompanyId)
                    .ToListAsync();

                if (!userCompanies.Any())
                    throw new ForbiddenAccessException("User does not belong to any companies");
                break;

            default:
                if (!Guid.TryParse(companyId, out var companyGuid))
                    throw new InvalidCompanyFilterException($"Invalid company filter value: {companyId}");

                var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyGuid);
                if (!companyExists)
                    throw new CompanyNotFoundException($"Company with ID {companyId} not found");

                var hasAccess = await _context.UserCompanies
                    .AnyAsync(uc => uc.MemberId == memberId && uc.CompanyId == companyGuid);

                if (!hasAccess)
                    throw new ForbiddenAccessException($"User does not have access to company {companyId}");
                break;
        }
    }

    public async Task<bool> HasAdminPrivilegesAsync(Guid userId)
    {
        var roles = await _identityService.GetUserRolesAsync(userId.ToString());
        return roles.Contains("Admin") || roles.Contains("SuperAdmin");
    }

    public async Task<List<Guid>> GetAccessibleCompanyIdsAsync(Guid userId)
    {
        var memberId = await _context.Members.Where(m => m.UserId == userId)
            .Select(m => m.Id).FirstOrDefaultAsync();
        return await _context.UserCompanies
            .Where(uc => uc.MemberId == userId)
            .Select(uc => uc.CompanyId)
            .ToListAsync();
    }
}

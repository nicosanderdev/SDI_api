using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Reports;
using SDI_Api.Application.Util.Extensions;
using Sdi_Api.Application.Util;

namespace Sdi_Api.Application.Reports.Queries;

public class GetVisitsBySourceQuery : IRequest<List<VisitSourceDto>>
{
    public string Period { get; set; } = "last30days";
    public string? CompanyId { get; set; } // Company filter parameter
}

public class GetVisitsBySourceQueryHandler : IRequestHandler<GetVisitsBySourceQuery, List<VisitSourceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICompanyAccessService _companyAccessService;
    private readonly ICurrentUserService _currentUserService;

    // Example: Assign colors to sources. Could be DB driven or config driven.
    private static readonly Dictionary<string, string> SourceColors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "Web", "#4CAF50" },
        { "App", "#2196F3" },
        { "Referral", "#FFC107" },
        { "Direct", "#9E9E9E" },
        { "Organic Search", "#FF5722"}
    };

    public GetVisitsBySourceQueryHandler(
        IApplicationDbContext context,
        ICompanyAccessService companyAccessService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _companyAccessService = companyAccessService;
        _currentUserService = currentUserService;
    }

    public async Task<List<VisitSourceDto>> Handle(GetVisitsBySourceQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        // Validate company access
        await _companyAccessService.ValidateCompanyAccessAsync(userId, request.CompanyId);

        var (startDate, endDate) = PeriodParser.ParsePeriod(request.Period);

        // Get accessible company IDs for filtering
        var accessibleCompanyIds = await _companyAccessService.GetAccessibleCompanyIdsAsync(userId);

        var result = await _context.PropertyVisitLogs
            .Include(v => v.Property!)
            .ThenInclude(ep => ep.Owner)
            .Where(v => v.VisitedOnUtc >= startDate && v.VisitedOnUtc <= endDate && v.Source != null)
            .ApplyCompanyFilter(request.CompanyId, userId, accessibleCompanyIds)
            .GroupBy(v => v.Source!) // Source is not null due to Where clause
            .Select(g => new VisitSourceDto
            {
                Source = g.Key,
                Visits = g.Count(),
                Color = null // Placeholder, will be set below
            })
            .OrderByDescending(s => s.Visits)
            .ToListAsync(cancellationToken);

        foreach (var item in result)
        {
            SourceColors.TryGetValue(item.Source, out var color);
            item.Color = color ?? "#CCCCCC"; // Default color if not found
        }

        return result;
    }
}

using System.Globalization;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs;
using SDI_Api.Application.Util.Extensions;
using Sdi_Api.Application.Util;

namespace Sdi_Api.Application.Reports.Queries;

public class GetDailyVisitsQuery : IRequest<List<DailyVisitDto>>
{
    public string Period { get; set; } = "last7days";
    public string DateFormat { get; set; } = "yyyy-MM-dd"; // "yyyy-MM-dd" or "dd/MM" for chart
    public string? CompanyId { get; set; } // Company filter parameter
}

public class GetDailyVisitsQueryHandler : IRequestHandler<GetDailyVisitsQuery, List<DailyVisitDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICompanyAccessService _companyAccessService;
    private readonly ICurrentUserService _currentUserService;

    public GetDailyVisitsQueryHandler(
        IApplicationDbContext context,
        ICompanyAccessService companyAccessService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _companyAccessService = companyAccessService;
        _currentUserService = currentUserService;
    }

    public async Task<List<DailyVisitDto>> Handle(GetDailyVisitsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        // Validate company access
        await _companyAccessService.ValidateCompanyAccessAsync(userId, request.CompanyId);

        var (startDate, endDate) = PeriodParser.ParsePeriod(request.Period);

        var dateRange = Enumerable.Range(0, (endDate.Date - startDate.Date).Days + 1)
            .Select(offset => startDate.Date.AddDays(offset))
            .ToList();

        // Get accessible company IDs for filtering
        var accessibleCompanyIds = await _companyAccessService.GetAccessibleCompanyIdsAsync(userId);

        /* var dailyVisitsRaw = await _context.PropertyVisitLogs
            .Include(v => v.)
            .ThenInclude(ep => ep.Owner)
            .Where(v => v.VisitedOnUtc >= startDate && v.VisitedOnUtc <= endDate)
            .ApplyCompanyFilter(request.CompanyId, userId, accessibleCompanyIds)
            .GroupBy(v => v.VisitedOnUtc.Date)
            .Select(g => new { Date = g.Key, Visits = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Visits, cancellationToken); */

        var result = dateRange.Select(date => new DailyVisitDto
            {
                Date = date.ToString(request.DateFormat, CultureInfo.InvariantCulture),
                DayName = date.ToString("ddd", CultureInfo.CurrentCulture), // e.g., "Mon", "Tue" (localized)
                Visits = 0
            })
            .ToList();

        return result;
    }
}

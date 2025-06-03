using System.Globalization;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs;
using Sdi_Api.Application.Util;

namespace Sdi_Api.Application.Reports.Queries;

public class GetDailyVisitsQuery : IRequest<List<DailyVisitDto>>
{
    public string Period { get; set; } = "last7days";
    public string DateFormat { get; set; } = "yyyy-MM-dd"; // "yyyy-MM-dd" or "dd/MM" for chart
}

public class GetDailyVisitsQueryHandler : IRequestHandler<GetDailyVisitsQuery, List<DailyVisitDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDailyVisitsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DailyVisitDto>> Handle(GetDailyVisitsQuery request, CancellationToken cancellationToken)
    {
        var (startDate, endDate) = PeriodParser.ParsePeriod(request.Period);
            
        var dateRange = Enumerable.Range(0, (endDate.Date - startDate.Date).Days + 1)
            .Select(offset => startDate.Date.AddDays(offset))
            .ToList();

        var dailyVisitsRaw = await _context.PropertyVisitLogs
            .Where(v => v.VisitedOnUtc >= startDate && v.VisitedOnUtc <= endDate)
            .GroupBy(v => v.VisitedOnUtc.Date)
            .Select(g => new { Date = g.Key, Visits = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Visits, cancellationToken);

        var result = dateRange.Select(date => new DailyVisitDto
            {
                Date = date.ToString(request.DateFormat, CultureInfo.InvariantCulture),
                DayName = date.ToString("ddd", CultureInfo.CurrentCulture), // e.g., "Mon", "Tue" (localized)
                Visits = dailyVisitsRaw.TryGetValue(date, out var visits) ? visits : 0
            })
            .ToList();

        return result;
    }
}

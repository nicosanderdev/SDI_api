using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Reports;
using Sdi_Api.Application.Util;

namespace Sdi_Api.Application.Reports.Queries;

public class GetVisitsBySourceQuery : IRequest<List<VisitSourceDto>>
{
    public string Period { get; set; } = "last30days";
}

public class GetVisitsBySourceQueryHandler : IRequestHandler<GetVisitsBySourceQuery, List<VisitSourceDto>>
{
    private readonly IApplicationDbContext _context;

    // Example: Assign colors to sources. Could be DB driven or config driven.
    private static readonly Dictionary<string, string> SourceColors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "Web", "#4CAF50" },
        { "App", "#2196F3" },
        { "Referral", "#FFC107" },
        { "Direct", "#9E9E9E" },
        { "Organic Search", "#FF5722"}
    };

    public GetVisitsBySourceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<VisitSourceDto>> Handle(GetVisitsBySourceQuery request, CancellationToken cancellationToken)
    {
        var (startDate, endDate) = PeriodParser.ParsePeriod(request.Period);

        var result = await _context.PropertyVisitLogs
            .Where(v => v.VisitedOnUtc >= startDate && v.VisitedOnUtc <= endDate && v.Source != null)
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

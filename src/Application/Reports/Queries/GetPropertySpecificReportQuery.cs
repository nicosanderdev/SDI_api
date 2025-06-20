using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Reports;
using Sdi_Api.Application.Util;
using SDI_Api.Domain.Entities;

namespace Sdi_Api.Application.Reports.Queries;

public class GetPropertySpecificReportQuery : IRequest<PropertySpecificReportDataDto>
{
    public Guid PropertyId { get; set; }
    public string Period { get; set; } = "last30days";
}

public class GetPropertySpecificReportQueryHandler : IRequestHandler<GetPropertySpecificReportQuery, PropertySpecificReportDataDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPropertySpecificReportQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PropertySpecificReportDataDto> Handle(GetPropertySpecificReportQuery request, CancellationToken cancellationToken)
    {
        var property = await _context.EstateProperties
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken);

        if (property == null)
        {
            throw new NotFoundException(nameof(EstateProperty), request.PropertyId.ToString());
        }

        var (startDate, endDate) = PeriodParser.ParsePeriod(request.Period);
        var dateRange = Enumerable.Range(0, (endDate.Date - startDate.Date).Days + 1)
                                  .Select(offset => startDate.Date.AddDays(offset))
                                  .ToList();

        var visitTrendRaw = await _context.PropertyVisitLogs
            .Where(v => v.PropertyId == request.PropertyId && v.VisitedOnUtc >= startDate && v.VisitedOnUtc <= endDate)
            .GroupBy(v => v.VisitedOnUtc.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        
        var visitTrend = dateRange
            .GroupJoin(visitTrendRaw, dr => dr, vtr => vtr.Date, (dr, vtrGroup) => new DateCountDto {
                Date = dr.ToString("yyyy-MM-dd"),
                Count = vtrGroup.SingleOrDefault()?.Count ?? 0
            })
            .OrderBy(dc => dc.Date)
            .ToList();


        var messageTrendRaw = await _context.PropertyMessageLogs
            .Where(m => m.PropertyId == request.PropertyId && m.SentOnUtc >= startDate && m.SentOnUtc <= endDate)
            .GroupBy(m => m.SentOnUtc.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var messageTrend = dateRange
            .GroupJoin(messageTrendRaw, dr => dr, mtr => mtr.Date, (dr, mtrGroup) => new DateCountDto {
                Date = dr.ToString("yyyy-MM-dd"),
                Count = mtrGroup.SingleOrDefault()?.Count ?? 0
            })
            .OrderBy(dc => dc.Date)
            .ToList();

        // Conversion rate calculation (example: messages / visits for the period)
        decimal? conversionRate = null;
        long totalVisitsInPeriod = visitTrend.Sum(vt => vt.Count);
        long totalMessagesInPeriod = messageTrend.Sum(mt => mt.Count);
        if (totalVisitsInPeriod > 0)
        {
            conversionRate = (decimal)totalMessagesInPeriod / totalVisitsInPeriod;
        }

        return new PropertySpecificReportDataDto
        {
            PropertyDetails = _mapper.Map<PropertyDetailsForReportDto>(property),
            VisitTrend = visitTrend,
            MessageTrend = messageTrend,
            ConversionRate = conversionRate,
            AverageTimeToRespond = "N/A" // Placeholder - requires more complex logic/data
        };
    }
}

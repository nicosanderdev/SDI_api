using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs;

namespace Sdi_Api.Application.Reports.Queries;

public class GetMonthlySummaryQuery : IRequest<MonthlySummaryDataDto>
{
    public int Year { get; set; }
    public int Month { get; set; } // 1-12
}

public class GetMonthlySummaryQueryHandler : IRequestHandler<GetMonthlySummaryQuery, MonthlySummaryDataDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper; // If mapping from intermediate objects

    public GetMonthlySummaryQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MonthlySummaryDataDto> Handle(GetMonthlySummaryQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateTime(request.Year, request.Month, 1);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        var visitsByDay = await _context.PropertyVisitLogs
            .Where(v => v.VisitedOnUtc >= startDate && v.VisitedOnUtc <= endDate)
            .GroupBy(v => v.VisitedOnUtc.Date)
            .Select(g => new DateCountDto { Date = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() })
            .OrderBy(dc => dc.Date)
            .ToListAsync(cancellationToken);

        var messagesByDay = await _context.PropertyMessageLogs
            .Where(m => m.SentOnUtc >= startDate && m.SentOnUtc <= endDate)
            .GroupBy(m => m.SentOnUtc.Date)
            .Select(g => new DateCountDto { Date = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() })
            .OrderBy(dc => dc.Date)
            .ToListAsync(cancellationToken);
        
        // Fill missing dates with 0 counts for a complete month series
        var allDatesInMonth = Enumerable.Range(1, DateTime.DaysInMonth(request.Year, request.Month))
                                        .Select(day => new DateTime(request.Year, request.Month, day));

        IEnumerable<DateTime> datesInMonth = allDatesInMonth as DateTime[] ?? allDatesInMonth.ToArray();
        var completeVisits = datesInMonth
            .GroupJoin(visitsByDay, date => date.ToString("yyyy-MM-dd"), v => v.Date, (date, vGroup) => vGroup.SingleOrDefault() ?? new DateCountDto { Date = date.ToString("yyyy-MM-dd"), Count = 0 })
            .OrderBy(dc => dc.Date)
            .ToList();

        var completeMessages = datesInMonth
            .GroupJoin(messagesByDay, date => date.ToString("yyyy-MM-dd"), m => m.Date, (date, mGroup) => mGroup.SingleOrDefault() ?? new DateCountDto { Date = date.ToString("yyyy-MM-dd"), Count = 0 })
            .OrderBy(dc => dc.Date)
            .ToList();


        return new MonthlySummaryDataDto
        {
            Visits = completeVisits,
            Messages = completeMessages
        };
    }
}

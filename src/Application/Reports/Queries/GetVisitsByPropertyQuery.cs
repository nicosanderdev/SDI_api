using System.Globalization;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Reports;
using Sdi_Api.Application.Util;

namespace Sdi_Api.Application.Reports.Queries;

public class GetVisitsByPropertyQuery : IRequest<VisitsByPropertyDataDto>
{
    public string Period { get; set; } = "last30days";
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
}

public class GetVisitsByPropertyQueryHandler : IRequestHandler<GetVisitsByPropertyQuery, VisitsByPropertyDataDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetVisitsByPropertyQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<VisitsByPropertyDataDto> Handle(GetVisitsByPropertyQuery request, CancellationToken cancellationToken)
    {
        var (startDate, endDate) = PeriodParser.ParsePeriod(request.Period);

        var query = _context.PropertyVisitLogs
            .Where(v => v.VisitedOnUtc >= startDate && v.VisitedOnUtc <= endDate)
            .GroupBy(v => v.PropertyId)
            .Select(g => new 
            {
                PropertyId = g.Key,
                VisitCount = g.Count()
            })
            .Join(_context.EstateProperties, // Join with EstateProperties to get details
                  visitStat => visitStat.PropertyId,
                  prop => prop.Id,
                  (visitStat, prop) => new PropertyVisitStatDto // Project directly or use AutoMapper later
                  {
                      PropertyId = prop.Id.ToString(),
                      PropertyTitle = prop.Title,
                      Address = prop.StreetName + prop.HouseNumber, // Or a formatted address string
                      VisitCount = visitStat.VisitCount,
                      Price = prop.EstatePropertyValues.FirstOrDefault(epv => epv.IsFeatured)!.RentPrice ?? prop.EstatePropertyValues.FirstOrDefault(epv => epv.IsFeatured)!.SalePrice,
                      Status = prop.EstatePropertyValues.FirstOrDefault(epv => epv.IsFeatured)!.Status.ToString(),
                      // Messages, Trends, Conversion would require more complex queries or separate calculations
                      Messages = _context.PropertyMessageLogs.Count(m => m.PropertyId == prop.Id && m.SentOnUtc >= startDate && m.SentOnUtc <= endDate), // Example, can be intensive
                      // Trends and conversion are complex and typically calculated with more historical data or specific logic
                      VisitsTrend = "flat", // Placeholder
                      MessagesTrend = "flat", // Placeholder
                      Conversion = "0%", // Placeholder
                      ConversionTrend = "flat" // Placeholder
                  })
            .OrderByDescending(p => p.VisitCount);
            //.ThenBy(p => p.PropertyTitle); // Optional secondary sort

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);
        
        // Note: Calculating messages for each property within this query can be performance-intensive.
        // Consider if this data is critical here or fetched on demand.
        // Trends and conversion rates are significantly more complex and usually involve comparing
        // current period data with a previous period, which is beyond a simple LINQ query here.
        // They are set as placeholders.

        return new VisitsByPropertyDataDto
        {
            Data = items,
            Total = totalCount,
            Page = request.Page,
            Limit = request.Limit
        };
    }
}

using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Reports;
using Sdi_Api.Application.Util;
using SDI_Api.Application.Util.Extensions;
using SDI_Api.Domain.Entities;

namespace Sdi_Api.Application.Reports.Queries;

public class GetDashboardSummaryQuery : IRequest<DashboardSummaryDataDto>
{
    public string? Period { get; set; } // Optional, defaults in handler or PeriodParser
    public string? CompanyId { get; set; } // Company filter parameter
}

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDataDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICompanyAccessService _companyAccessService;
    private readonly ICurrentUserService _currentUserService;

    public GetDashboardSummaryQueryHandler(
        IApplicationDbContext context,
        ICompanyAccessService companyAccessService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _companyAccessService = companyAccessService;
        _currentUserService = currentUserService;
    }

    private async Task<long> GetVisitCountAsync(IQueryable<PropertyVisitLog> visitLogs, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await visitLogs.LongCountAsync(v => v.VisitedOnUtc >= startDate && v.VisitedOnUtc <= endDate, cancellationToken);
    }

    private async Task<long> GetMessageCountAsync(IQueryable<PropertyMessageLog> messageLogs, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await messageLogs.LongCountAsync(m => m.SentOnUtc >= startDate && m.SentOnUtc <= endDate, cancellationToken);
    }

    private async Task<long> GetPropertyCountAsync(IQueryable<EstateProperty> properties, CancellationToken cancellationToken)
    {
        // For properties, "period" might mean properties created in period, or just total active.
        // This example assumes total active properties, not tied to period.
        // If it should be period-dependent (e.g. new listings), adjust this.
        return await properties.LongCountAsync(p => p.EstatePropertyValues.FirstOrDefault(epv => epv.IsFeatured)!.IsPropertyVisible, cancellationToken);
    }


    private DashboardSummaryStatDto CalculateStat(long current, long previous)
    {
        decimal? percentageChange = null;
        string changeDirection = "neutral";

        if (previous > 0)
        {
            percentageChange = ((decimal)current - previous) / previous * 100;
        }
        else if (current > 0) // Previous was 0, current is positive
        {
            percentageChange = 100; // Or null, or a very large number to signify infinite increase
        }
        // else current is 0 and previous is 0, percentageChange remains null

        if (percentageChange.HasValue)
        {
            if (percentageChange > 0.5m) changeDirection = "increase";
            else if (percentageChange < -0.5m) changeDirection = "decrease";
        }
        else
        {
            if (current > 0 && previous == 0)
            {
                changeDirection = "increase";
            }
            percentageChange = decimal.Zero;
        }
        
        return new DashboardSummaryStatDto
        {
            CurrentPeriod = current,
            PercentageChange = percentageChange,
            ChangeDirection = changeDirection
        };
    }

    public async Task<DashboardSummaryDataDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        // Validate company access
        await _companyAccessService.ValidateCompanyAccessAsync(userId, request.CompanyId);

        var (currentStartDate, currentEndDate) = PeriodParser.ParsePeriod(request.Period ?? "last30days");
        var periodDuration = currentEndDate - currentStartDate;
        var previousStartDate = currentStartDate - periodDuration - TimeSpan.FromTicks(1); // Ensure no overlap if periods are contiguous
        var previousEndDate = currentStartDate.AddTicks(-1);

        // Get accessible company IDs for filtering
        var accessibleCompanyIds = await _companyAccessService.GetAccessibleCompanyIdsAsync(userId);

        // Apply company filtering to visit logs
        var visitLogsQuery = _context.PropertyVisitLogs
            .Include(v => v.Property!)
            .ThenInclude(ep => ep.Owner)
            .ApplyCompanyFilter(request.CompanyId, userId, accessibleCompanyIds);

        var messageLogsQuery = _context.PropertyMessageLogs
            .Include(m => m.Property!)
            .ThenInclude(ep => ep.Owner)
            .ApplyCompanyFilter(request.CompanyId, userId, accessibleCompanyIds);

        long currentVisits = await GetVisitCountAsync(visitLogsQuery, currentStartDate, currentEndDate, cancellationToken);
        long previousVisits = await GetVisitCountAsync(visitLogsQuery, previousStartDate, previousEndDate, cancellationToken);

        long currentMessages = await GetMessageCountAsync(messageLogsQuery, currentStartDate, currentEndDate, cancellationToken);
        long previousMessages = await GetMessageCountAsync(messageLogsQuery, previousStartDate, previousEndDate, cancellationToken);

        // Apply company filtering to properties for total count
        var propertiesQuery = _context.EstateProperties
            .Include(ep => ep.EstatePropertyValues)
            .Include(ep => ep.Owner)
            .ApplyCompanyFilter(request.CompanyId, userId, accessibleCompanyIds);

        // For TotalProperties, "period" might mean new properties in period, or just total active.
        // This example assumes total active properties, not comparing periods.
        long totalActiveProps = await GetPropertyCountAsync(propertiesQuery, cancellationToken);
        // If TotalProperties should also have a trend, you'd need to count active properties at the end of the *previous* period.
        // This is more complex as it requires a snapshot or creation date filtering.
        // For simplicity, TotalProperties stat here won't have PercentageChange.
        var totalPropertiesStat = new DashboardSummaryStatDto
        {
            CurrentPeriod = totalActiveProps,
            PercentageChange = 0,
            ChangeDirection = "neutral"
        };

        // Conversion Rate: (Total Messages / Total Visits) * 100
        // This calculation needs to be done for both current and previous periods to find a trend.
        DashboardSummaryStatDto? conversionRateStat = null;
        if (currentVisits > 0 || previousVisits > 0) // Only calculate if there's some activity
        {
            decimal currentConversion = (currentVisits > 0) ? (decimal)currentMessages / currentVisits * 100 : 0;
            decimal previousConversion = (previousVisits > 0) ? (decimal)previousMessages / previousVisits * 100 : 0;
            conversionRateStat = CalculateStat((long)Math.Round(currentConversion), (long)Math.Round(previousConversion)); // Cast to long for CalculateStat
            conversionRateStat.CurrentPeriod = (long)Math.Round(currentConversion); // Set the actual current rate
        }


        return new DashboardSummaryDataDto
        {
            Visits = CalculateStat(currentVisits, previousVisits),
            Messages = CalculateStat(currentMessages, previousMessages),
            TotalProperties = totalPropertiesStat,
            ConversionRate = conversionRateStat
        };
    }
}

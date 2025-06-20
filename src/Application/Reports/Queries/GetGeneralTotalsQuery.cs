using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Reports;
using SDI_Api.Domain.Enums;

namespace Sdi_Api.Application.Reports.Queries;

public class GetGeneralTotalsQuery : IRequest<GeneralTotalsDataDto> { }

public class GetGeneralTotalsQueryHandler : IRequestHandler<GetGeneralTotalsQuery, GeneralTotalsDataDto>
{
    private readonly IApplicationDbContext _context;

    public GetGeneralTotalsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GeneralTotalsDataDto> Handle(GetGeneralTotalsQuery request, CancellationToken cancellationToken)
    {
        var totalProperties = await _context.EstateProperties.CountAsync(cancellationToken);
        var totalVisits = await _context.PropertyVisitLogs.LongCountAsync(cancellationToken);
        var totalMessages = await _context.PropertyMessageLogs.LongCountAsync(cancellationToken);
            
        var activeListingStatuses = new[] { PropertyStatus.Sale, PropertyStatus.Rent };
        var activeListings = await _context.EstateProperties
            .CountAsync(p => p.IsPublic && activeListingStatuses.Contains(p.Status), cancellationToken);
            
        decimal? averagePrice = null;
        if (await _context.EstateProperties.AnyAsync(p => p.IsPublic && p.Status == PropertyStatus.Sale, cancellationToken))
        {
            averagePrice = await _context.EstateProperties
                .Where(p => p.IsPublic && p.Status == PropertyStatus.Sale)
                .AverageAsync(p => p.Price, cancellationToken);
        }
        
        return new GeneralTotalsDataDto
        {
            TotalProperties = totalProperties,
            TotalVisitsLifetime = totalVisits,
            TotalMessagesLifetime = totalMessages,
            ActiveListings = activeListings,
            AveragePrice = averagePrice
        };
    }
}

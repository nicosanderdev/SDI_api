using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.EstateProperties;
using SDI_Api.Application.Util;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.EstateProperties.Commands.Create;

public class DuplicateEstatePropertyCommand : IRequest<DuplicatedEstatePropertyDto>
{
    /// <summary>
    /// The ID of the property to be duplicated.
    /// </summary>
    public Guid OriginalPropertyId { get; set; }

    /// <summary>
    /// The ID of the user requesting the duplication. Used for authorization.
    /// </summary>
    public Guid UserId { get; set; }
}

public class DuplicateEstatePropertyCommandHandler : IRequestHandler<DuplicateEstatePropertyCommand, DuplicatedEstatePropertyDto>
{
    private readonly IApplicationDbContext _context;

    public DuplicateEstatePropertyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DuplicatedEstatePropertyDto> Handle(DuplicateEstatePropertyCommand request, CancellationToken cancellationToken)
    {
        var originalProperty = await _context.EstateProperties
            .AsNoTracking()
            .Include(p => p.PropertyImages
                .Where(pi => !pi.IsDeleted))
            .Include(p => p.PropertyVideos)
            .Include(p => p.EstatePropertyAmenities)
                .ThenInclude(epa => epa.Amenity)
            .Include(p => p.EstatePropertyValues
                .Where(epv => epv.IsFeatured))
            .FirstOrDefaultAsync(p => p.Id == request.OriginalPropertyId, cancellationToken);
        
        if (originalProperty == null)
            throw new NotFoundException(nameof(EstateProperty), request.OriginalPropertyId.ToString());
        
        var duplicatedProperty = new EstateProperty();
        
        originalProperty.duplicateScalarValues(duplicatedProperty);
        
        var featuredValue = originalProperty.EstatePropertyValues.FirstOrDefault(v => v.IsFeatured);
        if (featuredValue != null)
        {
            var newValues = new EstatePropertyValues
            {
                Description = featuredValue.Description,
                AvailableFrom = DateTime.SpecifyKind(featuredValue.AvailableFrom, DateTimeKind.Utc),
                ArePetsAllowed = featuredValue.ArePetsAllowed,
                Capacity = featuredValue.Capacity,
                Currency = featuredValue.Currency,
                SalePrice = featuredValue.SalePrice,
                RentPrice = featuredValue.RentPrice,
                HasCommonExpenses = featuredValue.HasCommonExpenses,
                CommonExpensesValue = featuredValue.CommonExpensesValue,
                IsElectricityIncluded = featuredValue.IsElectricityIncluded,
                IsWaterIncluded = featuredValue.IsWaterIncluded,
                IsPriceVisible = featuredValue.IsPriceVisible,
                Status = featuredValue.Status,
                IsActive = featuredValue.IsActive,
                IsPropertyVisible = featuredValue.IsPropertyVisible,
                IsFeatured = true
            };
            duplicatedProperty.EstatePropertyValues.Add(newValues);
        }
        
        foreach (var originalAmenityLink in originalProperty.EstatePropertyAmenities)
        {
            duplicatedProperty.EstatePropertyAmenities.Add(new EstatePropertyAmenity
            {
                AmenityId = originalAmenityLink.AmenityId 
            });
        }
        
        foreach (var originalImage in originalProperty.PropertyImages)
        {
            duplicatedProperty.PropertyImages.Add(new PropertyImage
            {
                Url = originalImage.Url,
                AltText = originalImage.AltText,
                IsMain = originalImage.IsMain
            });
        }
        
        foreach (var originalVideo in originalProperty.PropertyVideos)
        {
            duplicatedProperty.PropertyVideos.Add(new PropertyVideo
            {
                Url = originalVideo.Url,
                Title = originalVideo.Title,
                Description = originalVideo.Description
            });
        }
        
        _context.EstateProperties.Add(duplicatedProperty);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new DuplicatedEstatePropertyDto
        {
            NewPropertyId = duplicatedProperty.Id,
            Title = duplicatedProperty.Title
        };
    }
}

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
    private readonly IFileStorageService _fileStorageService;

    public DuplicateEstatePropertyCommandHandler(IApplicationDbContext context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    public async Task<DuplicatedEstatePropertyDto> Handle(DuplicateEstatePropertyCommand request, CancellationToken cancellationToken)
    {
        var originalProperty = await _context.EstateProperties
            .AsNoTracking()
            .Include(p => p.PropertyImages
                .Where(pi => !pi.IsDeleted))
            .Include(p => p.PropertyVideos)
            .Include(p => p.PropertyDocuments)
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
            var newValues = new EstatePropertyValues();
            featuredValue.duplicateScalarValues(newValues);
            duplicatedProperty.EstatePropertyValues.Add(newValues);
        }
        
        foreach (var originalAmenityLink in originalProperty.EstatePropertyAmenities)
        {
            duplicatedProperty.EstatePropertyAmenities.Add(new EstatePropertyAmenity
            {
                AmenityId = originalAmenityLink.AmenityId,
                EstatePropertyId = duplicatedProperty.Id,
            });
        }
        
        string basePath = "StoragePaths:PropertyImages";

        // Paths for the original and duplicated properties:
        string originalImagesPath = Path.Combine(basePath, "images", originalProperty.Id.ToString());
        string newImagesPath = Path.Combine(basePath, "images", duplicatedProperty.Id.ToString());
        
        if (!Directory.Exists(newImagesPath))
            Directory.CreateDirectory(newImagesPath);

        foreach (var originalImage in originalProperty.PropertyImages)
        {
            var copiedFile = await _fileStorageService.CopyFileAsync(
                originalImage.Url,
                "StoragePaths:PropertyImages",
                [duplicatedProperty.Id.ToString()]
            );
            
            var newImage = new PropertyImage
            {
                Id = Guid.NewGuid(),
                Url = copiedFile.RelativePath,
                AltText = originalImage.AltText,
                IsMain = originalImage.IsMain
            };

            duplicatedProperty.PropertyImages.Add(newImage);

            if (originalImage.IsMain)
                duplicatedProperty.MainImageId = newImage.Id;
        }
        
        foreach (var originalVideo in originalProperty.PropertyVideos)
        {
            duplicatedProperty.PropertyVideos.Add(new PropertyVideo
            {
                Id = Guid.NewGuid(),
                Url = originalVideo.Url,
                Title = originalVideo.Title,
                Description = originalVideo.Description
            });
        }

        foreach (var originalDocument in originalProperty.PropertyDocuments)
        {
            var copiedFile = await _fileStorageService.CopyFileAsync(
                originalDocument.Url!,
                "StoragePaths:PropertyDocuments",
                [duplicatedProperty.Id.ToString()]
            );
            
            var newDocument = new PropertyDocument
            {
                Id = Guid.NewGuid(),
                Name = originalDocument.Name,
                Url = copiedFile.RelativePath,
                FileType = originalDocument.FileType,
                IsPublic = originalDocument.IsPublic,
                EstatePropertyId = duplicatedProperty.Id,
                EstateProperty = duplicatedProperty
            };

            duplicatedProperty.PropertyDocuments.Add(newDocument);
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

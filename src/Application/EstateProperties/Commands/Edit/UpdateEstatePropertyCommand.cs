using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Http;
using SDI_Api.Application.Dtos;
using SDI_Api.Application.DTOs.EstateProperties; // Or your preferred exception library

namespace SDI_Api.Application.EstateProperties.Commands.Edit;

public record UpdateEstatePropertyCommand : IRequest<Unit>
{
    public CreateOrUpdateEstatePropertyDto? EstatePropertyDto { get; set; }
}

public class UpdateEstatePropertyCommandHandler : IRequestHandler<UpdateEstatePropertyCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;
    
    public UpdateEstatePropertyCommandHandler(IApplicationDbContext context, IMapper mapper, IFileStorageService fileStorageService)
    {
        _context = context;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
    }

    public async Task<Unit> Handle(UpdateEstatePropertyCommand command, CancellationToken cancellationToken)
    {
        if (command.EstatePropertyDto == null)
            throw new ArgumentNullException(nameof(command.EstatePropertyDto));

        var request = command.EstatePropertyDto;

        var entity = await _context.EstateProperties
            .Include(p => p.PropertyImages)
            .Include(p => p.PropertyDocuments)
            .Include(p => p.PropertyVideos)
            .Include(p => p.EstatePropertyValues)
            .Include(p => p.EstatePropertyAmenities)
                .ThenInclude(epa => epa.Amenity)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(EstateProperty), request.Id.ToString());
        
        _mapper.Map(request, entity);

        var propertyFolderId = entity.Id.ToString();
        await UpdateDocumentsAsync(entity, request.PropertyDocuments, propertyFolderId!);
        await UpdateImagesAsync(entity, request.PropertyImages, request.MainImageId, propertyFolderId!);
        await UpdateVideosAsync(entity, request.PropertyVideos);
        UpdatePropertyValue(entity, request);
        await UpdateAmenitiesAsync(entity, request.Amenities);
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task UpdateDocumentsAsync(EstateProperty entity, List<PropertyDocumentDto>? newDocuments, string propertyFolderId)
    {
        var incomingDocumentsIds = newDocuments?
            .Where(img => !string.IsNullOrWhiteSpace(img.Id))
            .Select(img => img.Id!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        var documentsToDelete = entity.PropertyDocuments
            .Where(img => !incomingDocumentsIds.Contains(img.Id.ToString()))
            .ToList();
        
        foreach (var oldDocument in documentsToDelete)
        {
            await _fileStorageService.DeleteFileAsync(oldDocument.Url);
            _context.PropertyDocuments.Remove(oldDocument);
        }
        entity.PropertyDocuments = entity.PropertyDocuments.Except(documentsToDelete).ToList();
        
        var existingDbDocumentsIds = entity.PropertyDocuments
            .Select(doc => doc.Id.ToString())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        
        var docExtensions = new[] { ".pdf", ".doc", ".docx" };
        if (newDocuments != null)
        {
            var newPropertyDocuments = new List<PropertyDocument>();
            foreach (var docFile in newDocuments)
            {
                if (docFile.File == null)
                    continue;
                
                if (!string.IsNullOrWhiteSpace(docFile.Id) && existingDbDocumentsIds.Contains(docFile.Id))
                    continue;
                
                var fileResult = await _fileStorageService.SaveFileAsync(
                    docFile.File, 
                    "StoragePaths:PropertyDocuments",
                    docExtensions, 
                    propertyFolderId
                );
                
                Guid parsedId;
                Guid.TryParse(docFile.Id, out parsedId);
                var propertyDocumentsToAdd = _mapper.Map<PropertyDocument>(docFile);
                propertyDocumentsToAdd.Id = parsedId;
                propertyDocumentsToAdd.Url = fileResult.RelativePath;
                propertyDocumentsToAdd.EstatePropertyId = entity.Id;
                propertyDocumentsToAdd.EstateProperty = entity;
                
                entity.PropertyDocuments.Add(propertyDocumentsToAdd);
                newPropertyDocuments.Add(propertyDocumentsToAdd);
            }
            if (newPropertyDocuments.Count > 0)
            {
                _context.PropertyDocuments.AddRange(newPropertyDocuments);
            }
        }
    }

    /// <summary>
    /// Updates the images associated with an EstateProperty.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="newImages"></param>
    /// <param name="mainImageUrl"></param>
    /// <param name="propertyFolderId"></param>
    private async Task UpdateImagesAsync(EstateProperty entity, List<PropertyImageDto>? newImages, string? mainImageUrl, string propertyFolderId)
    {
        var incomingImageIds = newImages?
            .Where(img => !string.IsNullOrWhiteSpace(img.Id))
            .Select(img => img.Id!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        var imagesToDelete = entity.PropertyImages
            .Where(img => !incomingImageIds.Contains(img.Id.ToString()))
            .ToList();
        
        foreach (var oldImage in imagesToDelete)
        {
            await _fileStorageService.DeleteFileAsync(oldImage.Url);
            _context.PropertyImages.Remove(oldImage);
        }
        entity.PropertyImages = entity.PropertyImages.Except(imagesToDelete).ToList();
        
        var existingDbImageIds = entity.PropertyImages
            .Select(img => img.Id.ToString())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        
        var imgExtensions = new[] { ".jpg", ".jpeg", ".png" };
        if (newImages != null)
        {
            var newPropertyImages = new List<PropertyImage>();
            foreach (var imgDto in newImages)
            {
                if (imgDto.File == null)
                    continue;
                
                if (!string.IsNullOrWhiteSpace(imgDto.Id) && existingDbImageIds.Contains(imgDto.Id))
                    continue;
                
                var fileResult = await _fileStorageService.SaveFileAsync(
                    imgDto.File, 
                    "StoragePaths:PropertyImages",
                    imgExtensions, 
                    propertyFolderId
                );
                
                Guid parsedId;
                Guid.TryParse(imgDto.Id, out parsedId);
                var propertyImageToAdd = _mapper.Map<PropertyImage>(imgDto);
                propertyImageToAdd.Id = parsedId;
                propertyImageToAdd.Url = fileResult.RelativePath;
                propertyImageToAdd.EstatePropertyId = entity.Id;
                propertyImageToAdd.EstateProperty = entity;
                
                entity.PropertyImages.Add(propertyImageToAdd);
                newPropertyImages.Add(propertyImageToAdd);
            }
            if (newPropertyImages.Count > 0)
            {
                _context.PropertyImages.AddRange(newPropertyImages);
            }
        }
        
        if (!string.IsNullOrEmpty(mainImageUrl))
        {
            foreach (var img in entity.PropertyImages)
                img.IsMain = false;
            
            var mainImage = entity.PropertyImages
                .FirstOrDefault(img => Path.GetFileName(img.Url) == mainImageUrl);
            
            if (mainImage != null)
                mainImage.IsMain = true;
        }
        
        // Set MainImageId only after determining which image is main
        var mainImageEntity = entity.PropertyImages.FirstOrDefault(i => i.IsMain);
        if (mainImageEntity != null)
        {
            entity.MainImageId = mainImageEntity.Id;
        }
        else if (entity.PropertyImages.Any())
        {
            // If no main image is set, make the first one main
            var firstImage = entity.PropertyImages.First();
            firstImage.IsMain = true;
            entity.MainImageId = firstImage.Id;
        }
        else
        {
            entity.MainImageId = null;
        }
    }
    
    private Task UpdateVideosAsync(EstateProperty entity, List<PropertyVideoDto>? newVideos)
    {
        var incomingVideoIds = newVideos?
            .Where(v => !string.IsNullOrWhiteSpace(v.Id))
            .Select(v => v.Id!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        var videosToDelete = entity.PropertyVideos
            .Where(v => !incomingVideoIds.Contains(v.Id.ToString()))
            .ToList();
        
        foreach (var oldVideo in videosToDelete)
        {
            _context.PropertyVideos.Remove(oldVideo);
        }
        entity.PropertyVideos = entity.PropertyVideos.Except(videosToDelete).ToList();
        
        var existingDbVideoIds = entity.PropertyVideos
            .Select(v => v.Id.ToString())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        
        if (newVideos != null)
        {
            var newPropertyVideos = new List<PropertyVideo>();
            foreach (var videoDto in newVideos)
            {
                if (!string.IsNullOrWhiteSpace(videoDto.Id) && existingDbVideoIds.Contains(videoDto.Id))
                {
                    var existing = entity.PropertyVideos.FirstOrDefault(v => v.Id.ToString() == videoDto.Id);
                    if (existing != null)
                    {
                        if (!string.IsNullOrWhiteSpace(videoDto.Url))
                            existing.Url = videoDto.Url!;
                        existing.Description = videoDto.Description;
                    }
                    continue;
                }
                
                if (!existingDbVideoIds.Contains(videoDto.Id!))
                {
                    Guid.TryParse(videoDto.Id, out var parsedId);
                    var videoToAdd = _mapper.Map<PropertyVideo>(videoDto);
                    videoToAdd.Id = parsedId;
                    videoToAdd.EstatePropertyId = entity.Id;
                    videoToAdd.EstateProperty = entity;
                    entity.PropertyVideos.Add(videoToAdd);
                    newPropertyVideos.Add(videoToAdd);
                }
            }
            if (newPropertyVideos.Count > 0)
            {
                _context.PropertyVideos.AddRange(newPropertyVideos);
            }
        }
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Save a new EstatePropertyValues record when any value attribute changes; otherwise do nothing.
    /// Keeps existing relationships intact. Latest value becomes featured.
    /// </summary>
    private void UpdatePropertyValue(EstateProperty entity, CreateOrUpdateEstatePropertyDto? valueDto)
    {
        var existingValue = entity.EstatePropertyValues.FirstOrDefault();

        if (valueDto == null)
        {
            // No incoming values => no change per new requirements.
            return;
        }

        // Normalize incoming values
        string? incomingDescription = string.IsNullOrWhiteSpace(valueDto.Description)
            ? null
            : valueDto.Description!.Trim();
        var incomingAvailableFrom = DateTime.SpecifyKind(valueDto.AvailableFrom, DateTimeKind.Utc);
        var incomingCapacity = valueDto.Capacity;
        var incomingCurrency = valueDto.Currency;
        var incomingSalePrice = valueDto.SalePrice;
        var incomingRentPrice = valueDto.RentPrice;
        var incomingHasCommonExpenses = valueDto.HasCommonExpenses;
        var incomingCommonExpensesValue = valueDto.CommonExpensesAmount;
        bool? incomingIsElectricityIncluded = valueDto.IsElectricityIncluded;
        bool? incomingIsWaterIncluded = valueDto.IsWaterIncluded;
        var incomingIsPriceVisible = valueDto.IsPriceVisible;
        var incomingStatus = valueDto.Status;
        var incomingIsActive = valueDto.IsActive;
        var incomingIsPropertyVisible = valueDto.IsPropertyVisible;

        bool hasChanges = false;
        if (existingValue == null)
        {
            hasChanges = true; // no previous values, so we need to create one
        }
        else
        {
            // Compare all relevant fields
            hasChanges =
                Normalize(existingValue.Description) != incomingDescription ||
                NormalizeDate(existingValue.AvailableFrom) != incomingAvailableFrom ||
                existingValue.Capacity != incomingCapacity ||
                existingValue.Currency != incomingCurrency ||
                existingValue.SalePrice != incomingSalePrice ||
                existingValue.RentPrice != incomingRentPrice ||
                existingValue.HasCommonExpenses != incomingHasCommonExpenses ||
                existingValue.CommonExpensesValue != incomingCommonExpensesValue ||
                CoerceBool(existingValue.IsElectricityIncluded) != CoerceBool(incomingIsElectricityIncluded) ||
                CoerceBool(existingValue.IsWaterIncluded) != CoerceBool(incomingIsWaterIncluded) ||
                existingValue.IsPriceVisible != incomingIsPriceVisible ||
                existingValue.Status != incomingStatus ||
                existingValue.IsActive != incomingIsActive ||
                existingValue.IsPropertyVisible != incomingIsPropertyVisible;
        }

        if (!hasChanges)
        {
            // No value fields changed; do not update or create
            return;
        }

        // Create a new values record and mark it as featured
        var newValues = new EstatePropertyValues
        {
            Description = incomingDescription,
            AvailableFrom = incomingAvailableFrom,
            Capacity = incomingCapacity,
            Currency = incomingCurrency,
            SalePrice = incomingSalePrice,
            RentPrice = incomingRentPrice,
            HasCommonExpenses = incomingHasCommonExpenses,
            CommonExpensesValue = incomingCommonExpensesValue,
            IsElectricityIncluded = incomingIsElectricityIncluded,
            IsWaterIncluded = incomingIsWaterIncluded,
            IsPriceVisible = incomingIsPriceVisible,
            Status = incomingStatus,
            IsActive = incomingIsActive,
            IsPropertyVisible = incomingIsPropertyVisible,
            IsFeatured = true,
            EstatePropertyId = entity.Id,
            EstateProperty = entity
        };

        // Optionally un-feature the previous one to ensure a single featured record
        if (existingValue != null && existingValue.IsFeatured)
        {
            existingValue.IsFeatured = false;
        }

        entity.EstatePropertyValues.Add(newValues);
        _context.EstatePropertyValues.Add(newValues);

        static string? Normalize(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        static DateTime NormalizeDate(DateTime dt) => DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        static bool CoerceBool(bool? b) => b ?? false;
    }

    private async Task UpdateAmenitiesAsync(EstateProperty entity, List<AmenityDto>? newAmenities)
    {
        if (newAmenities == null)
            return;

        var incomingIds = newAmenities
            .Where(a => Guid.TryParse(a.Id, out _))
            .Select(a => Guid.Parse(a.Id!))
            .ToHashSet();

        var currentIds = entity.EstatePropertyAmenities
            .Select(ea => ea.AmenityId)
            .ToHashSet();

        // Remove old relationships
        var toRemove = entity.EstatePropertyAmenities
            .Where(ea => !incomingIds.Contains(ea.AmenityId))
            .ToList();

        foreach (var rel in toRemove)
        {
            entity.EstatePropertyAmenities.Remove(rel);
        }

        // Add new relationships
        var toAddIds = incomingIds.Except(currentIds).ToList();
        foreach (var id in toAddIds)
        {
            // only add if amenity exists and not deleted
            bool exists = await _context.Amenities.AnyAsync(a => a.Id == id && !a.IsDeleted);
            if (exists)
            {
                entity.EstatePropertyAmenities.Add(new EstatePropertyAmenity
                {
                    EstatePropertyId = entity.Id,
                    AmenityId = id,
                    CreatedAtUtc = DateTimeOffset.Now
                });
            }
        }
    }
}

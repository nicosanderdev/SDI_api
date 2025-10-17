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
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(EstateProperty), request.Id.ToString());
        
        _mapper.Map(request, entity);

        var propertyFolderId = entity.Id.ToString();
        await UpdateDocumentsAsync(entity, request.PropertyDocuments, propertyFolderId!);
        await UpdateImagesAsync(entity, request.PropertyImages, request.MainImageId, propertyFolderId!);
        await UpdateVideosAsync(entity, request.PropertyVideos);
        UpdatePropertyValue(entity, request);
        
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
    /// Manages the single EstatePropertyValue associated with an EstateProperty.
    /// It creates, updates, or deletes the value object based on the provided DTO.
    /// </summary>
    private void UpdatePropertyValue(EstateProperty entity, CreateOrUpdateEstatePropertyDto? valueDto)
    {
        var existingValue = entity.EstatePropertyValues.FirstOrDefault();

        if (valueDto != null)
        {
            if (existingValue != null)
            {
                valueDto.Id = existingValue.Id;
                _mapper.Map(valueDto, existingValue);
                existingValue.AvailableFrom = DateTime.SpecifyKind(existingValue.AvailableFrom, DateTimeKind.Utc);
            }
            else
            {
                var newValue = _mapper.Map<EstatePropertyValues>(valueDto);
                newValue.IsFeatured = true;
                newValue.AvailableFrom = DateTime.SpecifyKind(newValue.AvailableFrom, DateTimeKind.Utc);
                entity.EstatePropertyValues.Add(newValue);
            }
        }
        else if (existingValue != null)
            _context.EstatePropertyValues.Remove(existingValue);
    }
}

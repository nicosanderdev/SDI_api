using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Http;
using SDI_Api.Application.Dtos; // Or your preferred exception library

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
            .Include(p => p.Documents)
            .Include(p => p.EstatePropertyValues)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(EstateProperty), request.Id.ToString());
        
        _mapper.Map(request, entity);
        
        var propertyFolderId = GetOrGeneratePropertyFolderId(entity);
        await UpdateDocumentsAsync(entity, request.Documents, propertyFolderId);

        await UpdateImagesAsync(entity, request.PropertyImages, request.MainImageId, propertyFolderId);
        UpdatePropertyValue(entity, request);
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private string GetOrGeneratePropertyFolderId(EstateProperty entity)
    {
        var anyFileUrl = entity.PropertyImages.FirstOrDefault()?.Url ?? entity.Documents.FirstOrDefault()?.Url;

        if (!string.IsNullOrEmpty(anyFileUrl))
        {
            var pathSegments = anyFileUrl.Split(new[] { '/', '\\' });
            if (pathSegments.Length >= 2)
                return pathSegments[pathSegments.Length - 2];
        }
        return Guid.NewGuid().ToString();
    }

    private async Task UpdateDocumentsAsync(EstateProperty entity, List<IFormFile> newDocuments, string propertyFolderId)
    {
        foreach (var oldDoc in entity.Documents)
            await _fileStorageService.DeleteFileAsync(oldDoc.Url);
        _context.PropertyDocuments.RemoveRange(entity.Documents);
        entity.Documents.Clear();
        
        var docExtensions = new[] { ".pdf", ".doc", ".docx" };
        foreach (var docFile in newDocuments)
        {
            var fileResult = await _fileStorageService.SaveFileAsync(
                docFile, 
                "StoragePaths:PropertyDocuments",
                docExtensions, 
                propertyFolderId
            );
            
            entity.Documents.Add(new PropertyDocument {
                Name = fileResult.FileName,
                FileType = fileResult.ContentType,
                Url = fileResult.RelativePath
            });
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
        var incomingImageNames = newImages?.Where(img => img.File != null)
            .Select(img => img.File!.FileName)
            .ToHashSet() ?? new HashSet<string>();
        
        var imagesToDelete = entity.PropertyImages
            .Where(img => !incomingImageNames.Contains(Path.GetFileName(img.Url)))
            .ToList();
        
        foreach (var oldImage in imagesToDelete)
        {
            await _fileStorageService.DeleteFileAsync(oldImage.Url);
            _context.PropertyImages.Remove(oldImage);
        }
        entity.PropertyImages = entity.PropertyImages.Except(imagesToDelete).ToList();
        
        var existingImageNames = entity.PropertyImages
            .Select(img => Path.GetFileName(img.Url))
            .ToHashSet();
        
        var imgExtensions = new[] { ".jpg", ".jpeg", ".png" };
        if (newImages != null)
        {
            foreach (var imgDto in newImages)
            {
                if (imgDto.File == null)
                    continue;
                
                if (existingImageNames.Contains(imgDto.File.FileName))
                    continue;
                    
                var fileResult = await _fileStorageService.SaveFileAsync(
                    imgDto.File, 
                    "StoragePaths:PropertyImages",
                    imgExtensions, 
                    propertyFolderId
                );

                var propertyImageToAdd = new PropertyImage
                {
                    AltText = imgDto.AltText ?? fileResult.FileName, 
                    Url = fileResult.RelativePath,
                    IsMain = fileResult.FileName == mainImageUrl || (imgDto.IsMain != null && imgDto.IsMain.Value)
                };
                
                entity.PropertyImages.Add(propertyImageToAdd);
            }
        }

        // Now set IsMain flags and MainImageId after all images are added
        if (!string.IsNullOrEmpty(mainImageUrl))
        {
            foreach (var img in entity.PropertyImages)
                img.IsMain = false;
            
            var mainImage = entity.PropertyImages
                .FirstOrDefault(img => Path.GetFileName(img.Url) == mainImageUrl);
            
            if (mainImage != null)
            {
                mainImage.IsMain = true;
            }
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

using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Http; // Or your preferred exception library

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
        await UpdateImagesAsync(entity, request.Images, request.MainImageUrl, propertyFolderId);
        UpdatePropertyValue(entity, request);
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private string GetOrGeneratePropertyFolderId(EstateProperty entity)
    {
        // Try to extract folder ID from an existing file URL
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

        // 2. Add new documents, same as in Create
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

    private async Task UpdateImagesAsync(EstateProperty entity, List<IFormFile> newImages, string? mainImageUrl, string propertyFolderId)
    {
        foreach (var oldImage in entity.PropertyImages)
            await _fileStorageService.DeleteFileAsync(oldImage.Url);
        
        _context.PropertyImages.RemoveRange(entity.PropertyImages);
        entity.PropertyImages.Clear();
        entity.MainImageId = null;
        
        var imgExtensions = new[] { ".jpg", ".jpeg", ".png" };
        foreach (var imgFile in newImages)
        {
            var fileResult = await _fileStorageService.SaveFileAsync(
                imgFile, 
                "StoragePaths:PropertyImages",
                imgExtensions, 
                propertyFolderId
            );

            var propertyImageToAdd = new PropertyImage
            {
                AltText = fileResult.FileName, 
                Url = fileResult.RelativePath
            };
            
            if (fileResult.FileName == mainImageUrl)
            {
                propertyImageToAdd.IsMain = true;
                entity.MainImageId = propertyImageToAdd.Id;
            }
            entity.PropertyImages.Add(propertyImageToAdd);
        }

        // Ensure one image is marked as main
        if (!entity.PropertyImages.Any(i => i.IsMain) && entity.PropertyImages.Any())
        {
            var firstImage = entity.PropertyImages.First();
            firstImage.IsMain = true;
            entity.MainImageId = firstImage.Id;
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

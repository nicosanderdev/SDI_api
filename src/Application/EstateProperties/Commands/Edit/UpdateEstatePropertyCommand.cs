using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.EstateProperties.Commands.Edit;

public class UpdateEstatePropertyCommand : IRequest<Unit>
{
    public CreateOrUpdateEstatePropertyDto EstateProperty { get; set; } = default!;
    public CreateOrUpdatePropertyImageDto? MainImage { get; set; }
    public List<CreateOrUpdatePropertyImageDto>? PropertyImages { get; set; }
    public string? FeaturedValuesId { get; set; }
    public CreateOrUpdateEstatePropertyValuesDto? FeaturedValues { get; set; }
}

public class UpdateEstatePropertyCommandHandler : IRequestHandler<UpdateEstatePropertyCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateEstatePropertyCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(UpdateEstatePropertyCommand command, CancellationToken cancellationToken)
    {
        var entity = await _context.EstateProperties
            .Include(p => p.PropertyImages)
            .Include(p => p.EstatePropertyValues)
            .FirstOrDefaultAsync(p => p.Id == command.EstateProperty.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(EstateProperty), command.EstateProperty.Id.ToString());
        }
        
        _mapper.Map(command.EstateProperty, entity);
        
        if (command.FeaturedValues != null)
        {
            UpdatePropertyValues(command, entity);
        }
        
        if (command.PropertyImages != null)
        {
            UpdatePropertyImages(entity, command.PropertyImages, command.MainImage);
        }
        else if (command.MainImage != null)
        {
            UpdatePropertyImages(entity,
                entity.PropertyImages.Select(pi => _mapper.Map<CreateOrUpdatePropertyImageDto>(pi)).ToList(),
                command.MainImage);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private void UpdatePropertyValues(UpdateEstatePropertyCommand command, EstateProperty entity)
    {
        Guid valueId = Guid.Empty;
        // Check if the provided featured value has an ID
        bool hasId = !string.IsNullOrEmpty(command.FeaturedValues!.Id) && 
                     Guid.TryParse(command.FeaturedValues.Id, out valueId);

        if (hasId)
        {
            var existingValues = entity.EstatePropertyValues.FirstOrDefault(v => v.Id == valueId);

            if (existingValues != null)
            {
                existingValues = _mapper.Map<EstatePropertyValues>(command.FeaturedValues);
                entity.Id = existingValues.Id;
            }
            else
            {
                throw new NotFoundException(nameof(EstatePropertyValues), valueId.ToString());
            }
        }
        else
        {
            var newValues = _mapper.Map<EstatePropertyValues>(command.FeaturedValues);
            newValues.EstatePropertyId = entity.Id;
            entity.EstatePropertyValues.Add(newValues);
            entity.Id = newValues.Id;
        }
    }
    
    private void UpdatePropertyImages(EstateProperty entity, List<CreateOrUpdatePropertyImageDto> imageDtos, CreateOrUpdatePropertyImageDto? mainImageDto)
    {
        var existingImages = entity.PropertyImages.ToList();
        var imagesToRemove = new List<PropertyImage>();
        Guid? newMainImageId = entity.MainImageId;
        
        if (mainImageDto != null)
        {
            if (!string.IsNullOrEmpty(mainImageDto.Id) && Guid.TryParse(mainImageDto.Id, out Guid mainImgGuid))
            {
                var existingMain = existingImages.FirstOrDefault(i => i.Id == mainImgGuid);
                if (existingMain != null)
                {
                    _mapper.Map(mainImageDto, existingMain);
                    existingMain.IsMain = true;
                    newMainImageId = existingMain.Id;
                }
                else // ID provided but not found, treat as new (or error)
                {
                    var newImg = _mapper.Map<PropertyImage>(mainImageDto);
                    newImg.EstatePropertyId = entity.Id;
                    newImg.IsMain = true;
                    entity.PropertyImages.Add(newImg);
                    newMainImageId = newImg.Id;
                }
            }
            else // New main image (no ID or invalid ID)
            {
                var newImg = _mapper.Map<PropertyImage>(mainImageDto);
                newImg.EstatePropertyId = entity.Id;
                newImg.IsMain = true;
                entity.PropertyImages.Add(newImg);
                newMainImageId = newImg.Id;
            }
        }
        
        // Set all current images to IsMain = false, new main image will override
        foreach (var img in entity.PropertyImages)
        {
            if(newMainImageId.HasValue && img.Id != newMainImageId)
                img.IsMain = false;
            else if (!newMainImageId.HasValue) // no main image specified yet
                img.IsMain = false;
        }
        if(newMainImageId.HasValue)
        {
             var mainImgToSet = entity.PropertyImages.FirstOrDefault(i => i.Id == newMainImageId);
             if(mainImgToSet != null) mainImgToSet.IsMain = true;
        }


        // Synchronize the rest of the images
        foreach (var imgEntity in existingImages)
        {
            // If the main image DTO was processed and it matches this entity, skip (already handled)
            if (mainImageDto != null && !string.IsNullOrEmpty(mainImageDto.Id) && Guid.TryParse(mainImageDto.Id, out Guid mainDtoId) && mainDtoId == imgEntity.Id)
                continue;

            var dto = imageDtos.FirstOrDefault(d => d.Id != null && Guid.TryParse(d.Id, out Guid dtoId) && dtoId == imgEntity.Id);
            if (dto == null) // Not in DTO list, mark for removal
            {
                imagesToRemove.Add(imgEntity);
            }
            else // Exists in DTO, update it
            {
                _mapper.Map(dto, imgEntity);
                if (newMainImageId.HasValue && imgEntity.Id == newMainImageId) imgEntity.IsMain = true; // Ensure main flag
                else if (dto.IsMain != null && dto.IsMain.Value && !newMainImageId.HasValue) // If this DTO claims to be main and no explicit main yet
                {
                     newMainImageId = imgEntity.Id;
                     imgEntity.IsMain = true;
                }
            }
        }

        foreach (var imgToRemove in imagesToRemove)
        {
            _context.PropertyImages.Remove(imgToRemove);
            entity.PropertyImages.Remove(imgToRemove);
            if (entity.MainImageId == imgToRemove.Id) entity.MainImageId = null; // Clear main image if it was deleted
        }

        // Add new images (those in DTO with no ID or ID not in existing)
        foreach (var imgDto in imageDtos)
        {
             // If the main image DTO was processed and it matches this DTO's content (e.g. URL), skip
            if (mainImageDto != null && mainImageDto.Url == imgDto.Url && string.IsNullOrEmpty(imgDto.Id))
            {
                var alreadyAddedMain = entity.PropertyImages.FirstOrDefault(pi => pi.Url == mainImageDto.Url && pi.Id == newMainImageId);
                if(alreadyAddedMain != null) continue;
            }

            bool isExisting = false;
            if (!string.IsNullOrEmpty(imgDto.Id) && Guid.TryParse(imgDto.Id, out Guid dtoId))
            {
                isExisting = existingImages.Any(ei => ei.Id == dtoId);
            }

            if (!isExisting && (string.IsNullOrEmpty(imgDto.Id) || !entity.PropertyImages.Any(pi => pi.Id.ToString() == imgDto.Id)))
            {
                var newImg = _mapper.Map<PropertyImage>(imgDto);
                newImg.EstatePropertyId = entity.Id;
                if (newMainImageId.HasValue && newImg.Id == newMainImageId) newImg.IsMain = true;
                else if (imgDto.IsMain != null && imgDto.IsMain.Value && !newMainImageId.HasValue) {
                    newImg.IsMain = true;
                    newMainImageId = newImg.Id;
                }
                entity.PropertyImages.Add(newImg);
            }
        }
        entity.MainImageId = newMainImageId;
         // Final pass to ensure only one main image
        bool mainFound = false;
        foreach(var img in entity.PropertyImages.OrderByDescending(i => i.Id == newMainImageId)) // Prioritize the designated one
        {
            if(img.IsMain)
            {
                if(mainFound) img.IsMain = false; // unset others
                mainFound = true;
            }
        }
        if(!mainFound && entity.PropertyImages.Any()) // if no main, set first one
        {
            entity.PropertyImages.First().IsMain = true;
            entity.MainImageId = entity.PropertyImages.First().Id;
        } else if (!entity.PropertyImages.Any()) {
            entity.MainImageId = null;
        }
    }
}

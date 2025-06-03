using System.Globalization;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Enums;

namespace SDI_Api.Application.EstateProperties.Commands.Edit;

public class UpdateEstatePropertyCommand : IRequest<Unit>
{
    public CreateOrUpdateEstatePropertyDto EstateProperty { get; set; } = default!;
    public CreateOrUpdatePropertyImageDto? MainImage { get; set; }
    public List<CreateOrUpdatePropertyImageDto>? PropertyImages { get; set; }
    public string? FeaturedDescriptionId { get; set; }
    public List<CreateOrUpdateEstatePropertyDescriptionDto>? EstatePropertyDescriptions { get; set; }
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
            .Include(p => p.EstatePropertyDescriptions)
            .FirstOrDefaultAsync(p => p.Id == command.EstateProperty.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(EstateProperty), command.EstateProperty.Id.ToString());
        }

        // Update simple properties if values are provided
        if (command.EstateProperty.Title != null) entity.Title = command.EstateProperty.Title;
        if (command.EstateProperty.Address != null) entity.Address = command.EstateProperty.Address;
        if (command.EstateProperty.Address2 != null) entity.Address2 = command.EstateProperty.Address2;
        if (command.EstateProperty.City != null) entity.City = command.EstateProperty.City;
        if (command.EstateProperty.State != null) entity.State = command.EstateProperty.State;
        if (command.EstateProperty.ZipCode != null) entity.ZipCode = command.EstateProperty.ZipCode;
        if (command.EstateProperty.Country != null) entity.Country = command.EstateProperty.Country;
        if (command.EstateProperty.IsPublic.HasValue) entity.IsPublic = command.EstateProperty.IsPublic.Value;
        if (command.EstateProperty.Type != null) entity.Type = command.EstateProperty.Type;
        entity.Bedrooms = command.EstateProperty.Bedrooms;
        entity.Bathrooms = command.EstateProperty.Bathrooms;
        if (command.EstateProperty.Visits.HasValue) entity.Visits = command.EstateProperty.Visits.Value;

        if (command.EstateProperty.Price != null)
        {
            if (decimal.TryParse(command.EstateProperty.Price.Replace("€", "").Replace(",", ""), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out decimal priceValue))
                entity.Price = priceValue;
        }

        if (command.EstateProperty.Area != null)
        {
            var areaParts = command.EstateProperty.Area.TrimEnd('²').Split(new[] { 'm', 'M' }, StringSplitOptions.RemoveEmptyEntries);
            if (areaParts.Length > 0 && decimal.TryParse(areaParts[0], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out decimal areaNum))
            {
                entity.AreaValue = areaNum;
                entity.AreaUnit = command.EstateProperty.Area.Contains("m²") ? "m²" : (areaParts.Length > 1 ? areaParts[1] : "m²");
            }
        }

        if (command.EstateProperty.Status != null && Enum.TryParse<PropertyStatus>(command.EstateProperty.Status, true, out var statusEnum))
        {
            entity.Status = statusEnum;
        }

        // Handle PropertyImages synchronization
        if (command.PropertyImages != null)
        {
            UpdatePropertyImages(entity, command.PropertyImages, command.MainImage);
        }
        else if (command.MainImage != null) // Only main image info changed
        {
            UpdatePropertyImages(entity,
                entity.PropertyImages.Select(pi => _mapper.Map<CreateOrUpdatePropertyImageDto>(pi)).ToList(),
                command.MainImage);
        }

        // Handle EstatePropertyDescriptions synchronization
        if (command.EstatePropertyDescriptions != null)
        {
            UpdatePropertyDescriptions(entity, command.EstatePropertyDescriptions);
        }

        // Handle FeaturedDescriptionId
        if (command.FeaturedDescriptionId != null) // Client explicitly sets this
        {
            if (command.FeaturedDescriptionId.Equals("null", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrEmpty(command.FeaturedDescriptionId))
            {
                entity.FeaturedDescriptionId = null;
            }
            else if (Guid.TryParse(command.FeaturedDescriptionId, out Guid featDescId))
            {
                // Ensure this description ID exists within the property's descriptions
                if (entity.EstatePropertyDescriptions.Any(d => d.Id == featDescId))
                {
                    entity.FeaturedDescriptionId = featDescId;
                }
                // else: ID provided does not belong to this property's descriptions. Error or ignore? For now, ignore.
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
    
    private void UpdatePropertyImages(EstateProperty entity, List<CreateOrUpdatePropertyImageDto> imageDtos, CreateOrUpdatePropertyImageDto? mainImageDto)
    {
        var existingImages = entity.PropertyImages.ToList();
        var imagesToRemove = new List<PropertyImage>();
        Guid? newMainImageId = entity.MainImageId;

        // Process main image DTO first if provided
        if (mainImageDto != null)
        {
            if (!string.IsNullOrEmpty(mainImageDto.Id) && Guid.TryParse(mainImageDto.Id, out Guid mainImgGuid)) // Existing main image potentially updated
            {
                var existingMain = existingImages.FirstOrDefault(i => i.Id == mainImgGuid);
                if (existingMain != null)
                {
                    _mapper.Map(mainImageDto, existingMain); // Update its properties
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
            {
                continue;
            }

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

    private void UpdatePropertyDescriptions(EstateProperty entity, List<CreateOrUpdateEstatePropertyDescriptionDto> descriptionDtos)
    {
        var existingDescriptions = entity.EstatePropertyDescriptions.ToList();
        var descriptionsToRemove = new List<EstatePropertyDescription>();

        foreach (var descEntity in existingDescriptions)
        {
            var dto = descriptionDtos.FirstOrDefault(d => d.Id != null && Guid.TryParse(d.Id, out Guid dtoId) && dtoId == descEntity.Id);
            if (dto == null)
            {
                descriptionsToRemove.Add(descEntity);
            }
            else
            {
                _mapper.Map(dto, descEntity); // Update
            }
        }

        foreach (var descToRemove in descriptionsToRemove)
        {
            _context.EstatePropertyDescriptions.Remove(descToRemove);
            entity.EstatePropertyDescriptions.Remove(descToRemove);
             if (entity.FeaturedDescriptionId == descToRemove.Id) entity.FeaturedDescriptionId = null;
        }

        foreach (var descDto in descriptionDtos)
        {
            bool isExisting = false;
            if (!string.IsNullOrEmpty(descDto.Id) && Guid.TryParse(descDto.Id, out Guid dtoId))
            {
               isExisting = existingDescriptions.Any(ed => ed.Id == dtoId);
            }

            if (!isExisting) // Add new
            {
                var newDesc = _mapper.Map<EstatePropertyDescription>(descDto);
                newDesc.EstatePropertyId = entity.Id;
                entity.EstatePropertyDescriptions.Add(newDesc);
            }
        }
    }
}

using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Events;

namespace SDI_Api.Application.EstateProperties.Commands.Create;

public class CreateEstatePropertyCommand : IRequest<CreateOrUpdateEstatePropertyDto>
{
    public CreateOrUpdateEstatePropertyDto? CreateOrUpdateEstatePropertyDto { get; set; } =
        new CreateOrUpdateEstatePropertyDto();
}

public class CreateEstatePropertyCommandHandler : IRequestHandler<CreateEstatePropertyCommand, CreateOrUpdateEstatePropertyDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;

    public CreateEstatePropertyCommandHandler(IApplicationDbContext context, IMapper mapper, IFileStorageService fileStorageService)
    {
        _context = context;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
    }

    public async Task<CreateOrUpdateEstatePropertyDto> Handle(CreateEstatePropertyCommand command, CancellationToken cancellationToken)
    {
        var request = command.CreateOrUpdateEstatePropertyDto;
        var estateProperty = _mapper.Map<EstateProperty>(request);
        
        var propertyId = Guid.NewGuid();

        // Process Documents
        var docExtensions = new[] { ".pdf", ".doc", ".docx" };
        foreach (var docDto in request!.PropertyDocuments!)
        {
            var fileResult = await _fileStorageService.SaveFileAsync(
                docDto.File!, 
                "StoragePaths:PropertyDocuments",
                docExtensions, 
                propertyId.ToString()
            );
            
            estateProperty.PropertyDocuments.Add(new PropertyDocument {
                Name = fileResult.FileName,
                EstatePropertyId = request.Id,
                FileType = fileResult.ContentType,
                Url = fileResult.RelativePath,
                IsPublic = docDto.IsPublic
            });
        }
        
        // Process Images
        var imgExtensions = new[] { ".jpg", ".jpeg", ".png" };
        if (request.PropertyImages != null)
        {
            foreach (var imgFile in request.PropertyImages)
            {
                var fileResult = await _fileStorageService.SaveFileAsync(
                    imgFile.File!, 
                    "StoragePaths:PropertyImages",
                    imgExtensions, 
                    propertyId.ToString()
                );

                var propertyImageToAdd = new PropertyImage
                {
                    AltText = fileResult.FileName, 
                    Url = fileResult.RelativePath
                };
                
                if (imgFile.IsMain != null && imgFile.IsMain.Value)
                {
                    propertyImageToAdd.IsMain = true;
                    estateProperty.MainImageId = propertyImageToAdd.Id;
                }
                estateProperty.PropertyImages.Add(propertyImageToAdd);
            }
        }
        
        // Process Videos
        if (request.PropertyVideos != null)
        {
            foreach (var videoDto in request.PropertyVideos)
            {
                var propertyVideoToAdd = new PropertyVideo
                {
                    IsDeleted = false,
                    Url = videoDto.Url!,
                    Title = videoDto.Title ?? null,
                    Description = videoDto.Description ?? null,
                    EstatePropertyId = estateProperty.Id,
                    EstateProperty = estateProperty
                };
                
                estateProperty.PropertyVideos.Add(propertyVideoToAdd);
            }
        }
        
        // Process Amenities
        var amenitiesDb = await _context.Amenities.ToListAsync(cancellationToken);
        if (request.Amenities != null)
        {
            foreach (var amenityDto in request.Amenities)
            {
                Guid.TryParse(amenityDto.Id, out var amenityId);
                var amenityToAdd = new EstatePropertyAmenity
                {
                    EstatePropertyId = estateProperty.Id,
                    EstateProperty = estateProperty,
                    AmenityId = amenityId,
                    Amenity = amenitiesDb.FirstOrDefault(a => a.Id == amenityId)!,
                };
                
                estateProperty.EstatePropertyAmenities.Add(amenityToAdd);
            }
        }
        
        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId.ToString() == request!.OwnerId, cancellationToken);
        if (member == null || member.IsDeleted)
            throw new NotFoundException(nameof(Member), request!.OwnerId!);
        
        estateProperty.OwnerId = member.Id;
        estateProperty.Owner = member;
        estateProperty.Id = propertyId;
        
        var featuredValues = _mapper.Map<EstatePropertyValues>(request);
        featuredValues.IsFeatured = true;
        featuredValues.AvailableFrom = DateTime.SpecifyKind(featuredValues.AvailableFrom, DateTimeKind.Utc);
        
        estateProperty.EstatePropertyValues.Add(featuredValues);
        _context.EstateProperties.Add(estateProperty);
        await _context.SaveChangesAsync(cancellationToken);
        
        estateProperty.AddDomainEvent(new EstatePropertyCreatedEvent(estateProperty));
        return request;
    }
}

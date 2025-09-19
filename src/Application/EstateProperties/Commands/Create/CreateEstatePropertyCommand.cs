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
        
        var propertyFolderId = Guid.NewGuid().ToString();

        // Process Documents
        var docExtensions = new[] { ".pdf", ".doc", ".docx" };
        foreach (var docFile in request!.Documents)
        {
            var fileResult = await _fileStorageService.SaveFileAsync(
                docFile, 
                "StoragePaths:PropertyDocuments",
                docExtensions, 
                propertyFolderId
            );
            
            estateProperty.Documents.Add(new PropertyDocument {
                Name = fileResult.FileName,
                FileType = fileResult.ContentType,
                Url = fileResult.RelativePath
            });
        }
        
        // Process Images
        var imgExtensions = new[] { ".jpg", ".jpeg", ".png" };
        if (request.Images != null)
        {
            foreach (var imgFile in request.Images)
            {
                var fileResult = await _fileStorageService.SaveFileAsync(
                    imgFile.File!, 
                    "StoragePaths:PropertyImages",
                    imgExtensions, 
                    propertyFolderId
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
        
        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId.ToString() == request!.OwnerId, cancellationToken);
        if (member == null || member.IsDeleted)
            throw new NotFoundException(nameof(Member), request!.OwnerId!);
        
        estateProperty.OwnerId = member.Id;
        estateProperty.Owner = member;

        var featuredValues = _mapper.Map<EstatePropertyValues>(request);
        featuredValues.IsFeatured = true;
        featuredValues.AvailableFrom = DateTime.SpecifyKind(featuredValues.AvailableFrom, DateTimeKind.Utc);
        
        estateProperty.EstatePropertyValues.Add(featuredValues);
        _context.EstateProperties.Add(estateProperty);
        await _context.SaveChangesAsync(cancellationToken);
        
        estateProperty.AddDomainEvent(new EstatePropertyCreatedEvent(estateProperty));
        return request!;
    }
}

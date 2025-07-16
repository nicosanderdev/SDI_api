using SDI_Api.Application.Dtos;
using SDI_Api.Application.DTOs.EstateProperties;
using SDI_Api.Application.EstateProperties.Commands;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Util.Profiles;

public class EstatePropertyProfile : Profile
{
    public EstatePropertyProfile()
    {
        // =================================================================
        // Mappings from ENTITY to DTO (For Reading Data)
        // =================================================================
        
        CreateMap<PropertyImage, PropertyImageDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        
        CreateMap<EstateProperty, PublicEstatePropertyDto>()
            .ForMember(dest => dest.MainImage, opt => 
                opt.MapFrom(src => src.MainImage))
            .ForMember(dest => dest.Images, opt => 
                opt.MapFrom(src => src.PropertyImages));

        // =================================================================
        // Mappings from DTO to ENTITY (For Writing/Updating Data)
        // =================================================================
        CreateMap<PublicEstatePropertyDto, EstateProperty>()
            .ForMember(dest => dest.PropertyImages, opt => 
                opt.Ignore());
        
        CreateMap<PublicEstatePropertyDto, EstatePropertyValues>()
            .ForMember(dest => dest.Description, opt => 
                opt.MapFrom(src => src.Description));
            
        CreateMap<EstateProperty, UsersEstatePropertyDto>()
            .ForMember(dest => dest.PropertyImages,
                opt => opt.MapFrom(src => src.PropertyImages
                    .Where(pi => !pi.IsDeleted)))
            .ForMember(dest => dest.MainImageUrl,
                opt => opt.MapFrom(src => src.PropertyImages
                    .Where(pi => !pi.IsDeleted && pi.IsMain)
                    .Select(pi => pi.Url)
                    .FirstOrDefault()))
            // map other members if needed
            .ReverseMap();

        CreateMap<CreateOrUpdateEstatePropertyDto, EstateProperty>()
            .ForMember(dest => dest.PropertyImages, opt =>
                opt.Ignore())
            .ForMember(dest => dest.Documents, opt => 
                opt.Ignore())
            .ForMember(dest => dest.Title, opt => 
                opt.MapFrom(src => src.Title));
        
        CreateMap<CreateOrUpdateEstatePropertyDto, EstatePropertyValues>()
            .ForMember(dest => dest.Description, opt => 
                opt.MapFrom(src => src.Description));

        /* CreateMap<CreateOrUpdatePropertyImageDto, PropertyImage>()
            .ForMember(dest => dest.Id, opt => {
                opt.PreCondition(src => !string.IsNullOrEmpty(src.Id) && Guid.TryParse(src.Id, out _));
                opt.MapFrom(src => Guid.Parse(src.Id!));
             })
            .ForMember(dest => dest.Id, opt => { opt.PreCondition(src => string.IsNullOrEmpty(src.Id)); opt.MapFrom(_ => Guid.NewGuid()); }) // Generate new Guid if Id is null/empty
            .ForMember(dest => dest.EstatePropertyId, opt => opt.Ignore()) // Set manually in handler
            .ForMember(dest => dest.EstateProperty, opt => opt.Ignore());

        CreateMap<CreateOrUpdateEstatePropertyDescriptionDto, EstatePropertyDescription>()
             .ForMember(dest => dest.Id, opt => {
                opt.PreCondition(src => !string.IsNullOrEmpty(src.Id) && Guid.TryParse(src.Id, out _));
                opt.MapFrom(src => Guid.Parse(src.Id!));
             })
            .ForMember(dest => dest.Id, opt => { opt.PreCondition(src => string.IsNullOrEmpty(src.Id)); opt.MapFrom(_ => Guid.NewGuid()); }) // Generate new Guid if Id is null/empty
            .ForMember(dest => dest.EstatePropertyId, opt => opt.Ignore()) // Set manually in handler
            .ForMember(dest => dest.EstateProperty, opt => opt.Ignore());

        CreateMap<PropertyImage, CreateOrUpdatePropertyImageDto>();

        CreateMap<EstatePropertyDto, EstateProperty>();

        CreateMap<EstatePropertyDto, EstatePropertyValues>()
            .ForMember(dest => dest.EstatePropertyId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description)); */
    }
}

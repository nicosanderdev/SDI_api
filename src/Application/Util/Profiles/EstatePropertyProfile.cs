using SDI_Api.Application.Dtos;
using SDI_Api.Application.EstateProperties.Commands;
using SDI_Api.Domain.Entities;
using YourProject.Dto.Properties;

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
        
        CreateMap<EstateProperty, EstatePropertyDto>()
            .ForMember(dest => dest.Description, opt => 
                opt.MapFrom(src => src.FeaturedValues!.Description))
            .ForMember(dest => dest.SalePrice, opt => 
                opt.MapFrom(src => src.FeaturedValues!.SalePrice))
            .ForMember(dest => dest.RentPrice, opt => 
                opt.MapFrom(src => src.FeaturedValues!.RentPrice))
            .ForMember(dest => dest.MainImage, opt => 
                opt.MapFrom(src => src.MainImage))
            .ForMember(dest => dest.Images, opt => 
                opt.MapFrom(src => src.PropertyImages));

        // =================================================================
        // Mappings from DTO to ENTITY (For Writing/Updating Data)
        // =================================================================
        
        CreateMap<EstatePropertyDto, EstateProperty>()
            .ForMember(dest => dest.FeaturedValues, opt => 
                opt.Ignore())
            .ForMember(dest => dest.PropertyImages, opt => 
                opt.Ignore());
        
        CreateMap<EstatePropertyDto, EstatePropertyValues>()
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

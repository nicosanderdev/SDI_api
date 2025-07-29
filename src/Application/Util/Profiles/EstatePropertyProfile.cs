using System.Globalization;
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
                opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<CreateOrUpdateEstatePropertyDto, EstateProperty>()
            .ForMember(dest => dest.PropertyImages, opt =>
                opt.Ignore())
            .ForMember(dest => dest.Documents, opt =>
                opt.Ignore())
            .ForMember(dest => dest.Title, opt =>
                opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.LocationLatitude, opt =>
                opt.MapFrom(src => src.Location!.Latitude))
            .ForMember(dest => dest.LocationLongitude, opt =>
                opt.MapFrom(src => src.Location!.Longitude))
            .ForMember(dest => dest.OwnerId, opt =>
                opt.MapFrom(src => src.OwnerId));
        
        CreateMap<CreateOrUpdateEstatePropertyDto, EstatePropertyValues>()
            .ForMember(dest => dest.Description, opt => 
                opt.MapFrom(src => src.Description));


        CreateMap<EstateProperty, UsersEstatePropertyDto>()
            .ForMember(dest => dest.Id, opt =>
                opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.PropertyImages,
                opt =>
                    opt.MapFrom(src => src.PropertyImages
                        .Where(pi => !pi.IsDeleted)))
            .ForMember(dest => dest.MainImageId,
                opt =>
                    opt.MapFrom(src => src.MainImageId))
            .ForMember(dest => dest.Location,
                opt =>
                    opt.MapFrom(src => new LocationDto
                    {
                        Latitude = (double) src.LocationLatitude, Longitude = (double) src.LocationLongitude
                    }));

        CreateMap<EstatePropertyValues, UsersEstatePropertyDto>()
            .ForMember(dest => dest.Id, opt => 
                opt.Ignore())
            .ForMember(dest => dest.SalePrice, opt =>
                opt.MapFrom(src => src.SalePrice))
            .ForMember(dest => dest.RentPrice, opt =>
                opt.MapFrom(src => src.RentPrice))
            .ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src => src.Status.ToString()))
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

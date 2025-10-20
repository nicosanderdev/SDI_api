using System.Globalization;
using Microsoft.AspNetCore.SignalR;
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
        // Mappings for Property Images, Documents, Videos, Amenities
        // =================================================================
        
        CreateMap<PropertyImage, PropertyImageDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

        CreateMap<PropertyDocument, PropertyDocumentDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        
        CreateMap<PropertyImageDto, PropertyImage>()
            .ForMember(dest => dest.Id, opt =>
            {
                opt.PreCondition(src => !string.IsNullOrWhiteSpace(src.Id) && Guid.TryParse(src.Id, out _));
                opt.MapFrom(src => Guid.Parse(src.Id!));
            });
        
        CreateMap<PropertyVideo, PropertyVideoDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        
        CreateMap<PropertyVideoDto, PropertyVideo>()
            .ForMember(dest => dest.Id, opt =>
            {
                opt.PreCondition(src => !string.IsNullOrWhiteSpace(src.Id) && Guid.TryParse(src.Id, out _));
                opt.MapFrom(src => Guid.Parse(src.Id!));
            });
        
        CreateMap<PropertyDocumentDto, PropertyDocument>()
            .ForMember(dest => dest.Id, opt =>
            {
                opt.PreCondition(src => !string.IsNullOrWhiteSpace(src.Id) && Guid.TryParse(src.Id, out _));
                opt.MapFrom(src => Guid.Parse(src.Id!));
            });

        CreateMap<EstatePropertyAmenity, AmenityDto>()
            .ForMember(dest => dest.Id, opt => 
                opt.MapFrom(src => src.Amenity.Id))
            .ForMember(dest => dest.Name, opt => 
                opt.MapFrom(src => src.Amenity.Name));
        
        CreateMap<Amenity, AmenityDto>()
            .ForMember(dest => dest.Id, opt =>
                opt.MapFrom(src => src.Id));
        
        CreateMap<AmenityDto, Amenity>()
            .ForMember(dest => dest.Id, opt =>
                opt.MapFrom(src => Guid.Parse(src.Id!)));
        
        // =================================================================
        // Mappings for Public Estate Properties data
        // =================================================================

        CreateMap<EstateProperty, PublicEstatePropertyDto>()
            .ForMember(dest => dest.Images, opt =>
                opt.Ignore())
            .ForMember(dest => dest.SalePrice,
                opt => opt.MapFrom(src =>
                    src.EstatePropertyValues.FirstOrDefault(v => v.IsFeatured)!.SalePrice))
            .ForMember(dest => dest.RentPrice,
                opt => opt.MapFrom(src =>
                    src.EstatePropertyValues.FirstOrDefault(v => v.IsFeatured)!.RentPrice))
            .ForMember(dest => dest.Description,
                opt => opt.MapFrom(src => src.EstatePropertyValues.FirstOrDefault(v => v.IsFeatured)!.Description))
            .ForMember(dest => dest.Amenities, opt =>
                opt.MapFrom(src => src.EstatePropertyAmenities));
        
        CreateMap<PublicEstatePropertyDto, EstateProperty>()
            .ForMember(dest => dest.PropertyImages, opt =>
                opt.Ignore())
            .ForMember(dest => dest.EstatePropertyAmenities, opt => opt.MapFrom(src =>
                src.Amenities!
                    .Select(a => new EstatePropertyAmenity { AmenityId = Guid.Parse(a.Id!) })
            ));

        CreateMap<PublicEstatePropertyDto, EstatePropertyValues>()
            .ForMember(dest => dest.Description, opt =>
                opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Id, opt =>
                opt.Ignore());

        // =================================================================
        // Mappings for Users Estate Property data
        // =================================================================
        
        CreateMap<EstateProperty, UsersEstatePropertyDto>()
            .ForMember(dest => dest.Id, opt =>
                opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.PropertyImages, opt =>
                opt.MapFrom(src => src.PropertyImages .Where(pi => !pi.IsDeleted)))
            .ForMember(dest => dest.PropertyVideos, opt =>
                opt.MapFrom(src => src.PropertyVideos.Where(pv => !pv.IsDeleted)))
            .ForMember(dest => dest.Amenities, opt => 
                opt.MapFrom(src => src.EstatePropertyAmenities))
            .ForMember(dest => dest.MainImageId, opt =>
                opt.MapFrom(src => src.MainImageId))
            .ForMember(dest => dest.Location, opt =>
                opt.MapFrom(src => new LocationDto
                { Latitude = (double) src.LocationLatitude, Longitude = (double) src.LocationLongitude }));

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

        CreateMap<UsersEstatePropertyDto, EstateProperty>()
            .ForMember(dest => dest.EstatePropertyAmenities, opt => opt.Ignore());
        
        // =================================================================
        // Mappings for Create/Update Estate Property data
        // =================================================================
        
        CreateMap<CreateOrUpdateEstatePropertyDto, EstateProperty>()
            .ForMember(dest => dest.PropertyImages, opt =>
                opt.Ignore())
            .ForMember(dest => dest.PropertyDocuments, opt =>
                opt.Ignore())
            .ForMember(dest => dest.PropertyVideos, opt => 
                opt.Ignore())
            .ForMember(dest => dest.MainImageId, opt =>
                opt.MapFrom(src => src.MainImageId != null ? Guid.Parse(src.MainImageId!) : Guid.Empty))
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
    }
}

using Sdi_Api.Application.Dtos;
using SDI_Api.Application.Dtos;
using SDI_Api.Application.EstateProperties.Commands;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Util.Profiles;

public class EstatePropertyProfile : Profile
{
    public EstatePropertyProfile()
    {
        CreateMap<PropertyImage, PropertyImageDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
            
            CreateMap<EstatePropertyDescription, EstatePropertyDescriptionDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

            CreateMap<EstateProperty, EstatePropertyDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.CreatedOnUtc.ToString("dd/MM/yyyy")))
                .ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src => 
                    src.MainImage != null ? src.MainImage.Url :
                    (src.PropertyImages != null && src.PropertyImages.Any(pi => pi.IsMain) ? src.PropertyImages.First(pi => pi.IsMain).Url :
                    (src.PropertyImages != null && src.PropertyImages.Any() ? src.PropertyImages.OrderBy(pi => pi.Id).First().Url : null))
                 ))
                .ForMember(dest => dest.FeaturedDescriptionId, opt => opt.MapFrom(src => src.FeaturedDescriptionId.HasValue ? src.FeaturedDescriptionId.Value.ToString() : null))
                .ForMember(dest => dest.MainImage, opt => opt.MapFrom(src => src.MainImage ?? (src.PropertyImages != null && src.PropertyImages.Any(pi => pi.IsMain) ? src.PropertyImages.First(pi => pi.IsMain) : null)))
                .ForMember(dest => dest.EstatePropertyDescriptions, opt => opt.MapFrom(src => src.EstatePropertyDescriptions));


            // DTO to Entity (for Create/Update Commands)
            // Note: Price, Area, Status parsing is handled in Command Handlers for more control.
            //       AutoMapper can do it too but might be less flexible for complex strings.

            CreateMap<CreateOrUpdatePropertyImageDto, PropertyImage>()
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

            // For Update command: mapping DTO to existing Entity
            // CreateMap<UpdateEstatePropertyCommand, EstateProperty>()
            // This is usually handled property by property in the handler for partial updates.
            // If you map directly, you might overwrite existing values with nulls from the command.
            // However, for nested DTOs to nested Entities, it's useful:
            CreateMap<PropertyImage, CreateOrUpdatePropertyImageDto>(); // For current state to DTO in update handler
    }
}

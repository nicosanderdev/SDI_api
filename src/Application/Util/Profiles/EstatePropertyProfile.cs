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
                // 1. Quita las conversiones .ToString(). Hazlas en la capa de presentación si es necesario.
                //    Si los IDs son GUID en la entidad y string en el DTO, AutoMapper lo convierte automáticamente.
                //    Si son int/long, lo mismo.
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) // Asumiendo que el DTO tiene un Id del mismo tipo o string
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString())) // .ToString() en un Enum SÍ suele funcionar

                // 2. No formatees fechas aquí. Devuelve el DateTime y que el cliente/UI lo formatee.
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.CreatedOnUtc)) // DTO.Created debe ser DateTime

                // 3. Simplifica radicalmente la lógica de la URL de la imagen.
                //    Esta versión es mucho más fácil de traducir a SQL.
                .ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src =>
                    (src.MainImage!.Url) ?? src.PropertyImages!.OrderBy(pi => !pi.IsMain).ThenBy(pi => pi.Id).FirstOrDefault()!.Url
                ))
                
                .ForMember(dest => dest.FeaturedDescriptionId, opt => opt.MapFrom(src => src.FeaturedDescriptionId))

                // 6. Este mapeo anidado es correcto, siempre y cuando tengas un mapeo definido para
                //    EstatePropertyDescription -> EstatePropertyDescriptionDto.
                .ForMember(dest => dest.EstatePropertyDescriptions, opt => opt.MapFrom(src => src.EstatePropertyDescriptions));
            

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
            
            CreateMap<PropertyImage, CreateOrUpdatePropertyImageDto>();
    }
}

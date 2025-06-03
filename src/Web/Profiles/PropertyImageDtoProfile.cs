using AutoMapper;
using SDI_Api.Application.Dtos;
using SDI_Api.Application.EstatePropertyDescriptions;

namespace SDI_Api.Web.Profiles;

public class PropertyImageDtoProfile : Profile
{
    public PropertyImageDtoProfile()
    {
        CreateMap<PropertyImageDto, SavePropertyImageRequest>();
    }
}

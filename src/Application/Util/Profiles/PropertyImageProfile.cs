using SDI_Api.Application.EstatePropertyDescriptions;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Util.Profiles;

public class PropertyImageProfile : Profile
{
    public PropertyImageProfile()
    {
        CreateMap<SavePropertyImageRequest, PropertyImage>();
    }
}

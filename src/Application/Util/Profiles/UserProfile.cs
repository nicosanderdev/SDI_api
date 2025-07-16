using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Users;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Util.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<IUser, UserDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.getId()))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.getPhoneNumber()))
            .ForMember(dest => dest.Password, opt => opt.Ignore());
        
        CreateMap<Member, UserDto>();
    }
}

using Sdi_Api.Application.DTOs.Profile;

namespace SDI_Api.Application.Util.Profiles;

 public class ProfileMappingProfile : Profile
{
    public ProfileMappingProfile()
    {
        // ApplicationUser to ProfileDataDto
        CreateMap<ApplicationUser, ProfileDataDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForPath(dest => dest.Address!.Street, opt => opt.MapFrom(src => src.Street))
            .ForPath(dest => dest.Address!.Street2, opt => opt.MapFrom(src => src.Street2))
            .ForPath(dest => dest.Address!.City, opt => opt.MapFrom(src => src.City))
            .ForPath(dest => dest.Address!.State, opt => opt.MapFrom(src => src.State))
            .ForPath(dest => dest.Address!.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
            .ForPath(dest => dest.Address!.Country, opt => opt.MapFrom(src => src.Country))
            .AfterMap((src, dest) => // Ensure Address object is created if any address field is present
            {
                if (src.Street != null || src.City != null || src.Country != null) // Add more checks if needed
                {
                    if (dest.Address == null) dest.Address = new AddressDto();
                }
            });


        // AddressDto to ApplicationUser (for updating user from DTO)
        CreateMap<AddressDto, ApplicationUser>()
            .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Street))
            .ForMember(dest => dest.Street2, opt => opt.MapFrom(src => src.Street2))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            // Ignore all other ApplicationUser members not part of AddressDto
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));


        // UpdateProfileDto to ApplicationUser (used selectively in handler, or with explicit member mapping if desired)
        // For now, the handler maps property by property for more control.
        // If you wanted AutoMapper to handle more of UpdateUserProfileCommand:
        /*
        CreateMap<UpdateProfileDto, ApplicationUser>()
            .ForMember(dest => dest.FirstName, opt => opt.Condition(src => src.FirstName != null))
            .ForMember(dest => dest.LastName, opt => opt.Condition(src => src.LastName != null))
            .ForMember(dest => dest.Title, opt => opt.Condition(src => src.Title != null))
            // Email and Phone are handled specially by UserManager
            .ForMember(dest => dest.Email, opt => opt.Ignore()) 
            .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
            // Address is mapped separately if ProfileUpdateData.Address is not null
            .ForPath(dest => dest.Street, opt => opt.MapFrom(src => src.Address != null ? src.Address.Street : default))
            // ... other address fields ...
            .ForAllOtherMembers(opt => opt.Ignore()); // Ignore everything else
        */
    }
}

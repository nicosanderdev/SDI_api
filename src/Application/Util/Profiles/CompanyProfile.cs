using SDI_Api.Application.DTOs.Company;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Util.Profiles;

public class CompanyProfile : Profile
{
    public CompanyProfile()
    {
        CreateMap<Domain.Entities.Company, CompanyDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new AddressDto
            {
                Street = src.Street,
                Street2 = src.Street2,
                City = src.City,
                State = src.State,
                PostalCode = src.PostalCode,
                Country = src.Country
            }))
            .AfterMap((src, dest) =>
            {
                // Only create Address object if at least one field is present
                if (string.IsNullOrWhiteSpace(src.Street) && 
                    string.IsNullOrWhiteSpace(src.City) && 
                    string.IsNullOrWhiteSpace(src.Country))
                {
                    dest.Address = null;
                }
            });

        CreateMap<Member, CompanyUserDto>()
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.Ignore());
    }
}


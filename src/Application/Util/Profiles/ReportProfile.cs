using System.Globalization;
using SDI_Api.Application.DTOs;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Util.Profiles;

public class ReportProfile : Profile
{
    public ReportProfile()
    {
        // Mapping EstateProperty to PropertyDetailsForReportDto
        CreateMap<EstateProperty, PropertyDetailsForReportDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.ToString("C", CultureInfo.GetCultureInfo("eur")))) // Adjust as needed
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => 
                // Example of concatenating address parts, adjust as needed
                string.Join(", ", new[] { src.Address, src.City, src.State }.Where(s => !string.IsNullOrEmpty(s)))
            ));

        // Other mappings if needed, e.g., from intermediate query results to DTOs,
        // but many queries above project directly to DTOs.
    }
}

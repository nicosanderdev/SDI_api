using System.Globalization;
using SDI_Api.Application.DTOs.Reports;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Util.Profiles;

public class ReportProfile : Profile
{
    public ReportProfile()
    {
        // Mapping EstateProperty to PropertyDetailsForReportDto
        CreateMap<EstateProperty, PropertyDetailsForReportDto>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Price,
                opt => opt.MapFrom(src => src.EstatePropertyValues
                    .FirstOrDefault(v => v.IsFeatured)!.SalePrice.ToString()))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.EstatePropertyValues
                    .FirstOrDefault(v => v.IsFeatured)!.Status.ToString()))
            .ForMember(dest => dest.Address,
                opt => opt.MapFrom(src =>
                    string.Join(", ", new[] { src.StreetName + src.HouseNumber, src.City, src.State }
                        .Where(s => !string.IsNullOrEmpty(s)))));
    }
}

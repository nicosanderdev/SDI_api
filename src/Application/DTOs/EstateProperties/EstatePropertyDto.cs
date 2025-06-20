using SDI_Api.Application.Dtos;

namespace Sdi_Api.Application.Dtos;

public class EstatePropertyDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public string? Address { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public string? Area { get; set; }
    public bool Visits { get; set; } = false;
    public bool IsPublic { get; set; }
    public string? OwnerId { get; set; }
    public bool? IsDeleted { get; set; }
    public string? MainImageUrl { get; set; }
    public DateTime Created { get; set; }

    public PropertyImageDto? MainImage { get; set; }
    public List<PropertyImageDto>? PropertyImages { get; set; }
    public string? FeaturedDescriptionId { get; set; }
    public EstatePropertyDescriptionDto? FeaturedDescription { get; set; }
    public List<EstatePropertyDescriptionDto>? EstatePropertyDescriptions { get; set; }
    
}

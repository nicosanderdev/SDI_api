namespace SDI_Api.Application.EstateProperties.Commands;

public class CreateOrUpdateEstatePropertyDto
{
    public Guid Id { get; set; }
    public string? Address { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public bool? IsPublic { get; set; } = true; // Default from TS

    public string? Title { get; set; }
    public string? Area { get; set; }
    public string? Price { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    // 'Created' will be set to DateTime.UtcNow by entity default, or parsed if provided.
    // For create, typically we don't pass 'Created' date from client.
    // public string? Created { get; set; } 
    public int? Visits { get; set; }

    public CreateOrUpdatePropertyImageDto? MainImage { get; set; } // Client might send main image details separately
    public List<CreateOrUpdatePropertyImageDto> PropertyImages { get; set; } = new();
    public string? FeaturedDescriptionId { get; set; } // Usually set after descriptions are created
    public List<CreateOrUpdateEstatePropertyDescriptionDto> EstatePropertyDescriptions { get; set; } = new();
}

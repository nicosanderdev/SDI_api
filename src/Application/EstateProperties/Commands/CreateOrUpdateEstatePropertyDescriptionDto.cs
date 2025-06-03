namespace SDI_Api.Application.EstateProperties.Commands;

public class CreateOrUpdateEstatePropertyDescriptionDto
{
    public string? Id { get; set; }
    public string? EstatePropertyId { get; set; }
    public DateTimeOffset? AvailableFrom { get; set; }
    public DateTimeOffset? AvailableTo { get; set; }
    public long ListedPrice { get; set; }
    public long RentPrice { get; set; }
    public long SoldPrice { get; set; }
    public bool IsActive { get; set; } = false;
}

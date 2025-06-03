namespace SDI_Api.Application.EstateProperties.Commands;

public class CreatePropertyDescriptionRequest
{
    public Guid EstatePropertyId { get; set; }
    public int NumberOfPeople { get; set; }
    public int NumberOfBedrooms { get; set; }
    public int NumberOfBathrooms { get; set; }
    public DateTimeOffset? AvailableFrom { get; set; }
    public long ListedPrice { get; set; }
    public long RentPrice { get; set; }
    public long SoldPrice { get; set; }
    public bool IsActive { get; set; } = false;
}

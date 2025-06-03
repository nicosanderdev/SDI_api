namespace SDI_Api.Application.Dtos;

public class EstatePropertyDescriptionDto
{
    public Guid Id { get; set; }
    public Guid EstatePropertyId { get; set; }
    public DateTimeOffset? AvailableFrom { get; set; }
    public DateTimeOffset? AvailableTo { get; set; }
    public long ListedPrice { get; set; }
    public long RentPrice { get; set; }
    public long SoldPrice { get; set; }
    public bool IsActive { get; set; } = false;
}

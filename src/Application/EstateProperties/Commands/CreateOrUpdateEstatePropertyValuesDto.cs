using SDI_Api.Domain.Enums;

namespace SDI_Api.Application.EstateProperties.Commands;

public class CreateOrUpdateEstatePropertyValuesDto
{
    public string? Id { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? AvailableFrom { get; set; }
    public bool ArePetsAllowed { get; set; }
    public int Capacity { get; set; }
    public Currency Currency { get; set; }
    public long SalePrice { get; set; }
    public long RentPrice { get; set; }
    public bool HasCommonExpenses { get; set; }
    public decimal CommonExpensesAmount { get; set; }
    public bool IsElectricityIncluded { get; set; }
    public bool IsWaterIncluded { get; set; }
    public bool IsPriceVisible { get; set; }
    public PropertyStatus PropertyStatus { get; set; }
    public bool IsActive { get; set; }
    public bool IsPropertyVisible { get; set; }
    public string? EstatePropertyId { get; set; }
}

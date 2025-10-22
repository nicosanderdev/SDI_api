using SDI_Api.Domain.Enums;

namespace SDI_Api.Application.DTOs.EstateProperties;

public class EstatePropertyValuesDto
{
    public string? Description { get; set; }
    public DateTime AvailableFrom { get; set; }
    public int Capacity { get; set; }
    public Currency Currency { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? RentPrice { get; set; }
    public bool HasCommonExpenses { get; set; }
    public decimal? CommonExpensesValue { get; set; }
    public bool? IsElectricityIncluded { get; set; }
    public bool? IsWaterIncluded { get; set; }
    public bool IsPriceVisible { get; set; }
    public PropertyStatus Status { get; set; }
    public bool IsActive { get; set; }
    public bool IsPropertyVisible { get; set; }
    public bool IsFeatured { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Domain.Entities;

public class EstatePropertyValues : BaseAuditableEntity
{
    [MaxLength(1000)]
    public string? Description { get; set; }
    [Required]
    public DateTime AvailableFrom { get; set; }
    public bool ArePetsAllowed { get; set; }
    public int Capacity { get; set; }
    
    // Price and status
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
    
    // Relationships
    public Guid EstatePropertyId { get; set; }
    public virtual EstateProperty EstateProperty { get; set; } = null!; 

    public EstatePropertyValues()
    {
        Id = Guid.NewGuid();
    }
}

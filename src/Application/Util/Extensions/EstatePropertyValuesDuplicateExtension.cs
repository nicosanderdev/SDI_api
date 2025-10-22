using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Util;

public static class EstatePropertyValuesDuplicateExtension
{
    /// <summary>
    /// Copies scalar values from the source EstatePropertyValues into the target EstatePropertyValues,
    /// excluding Id and auditable fields (Created, CreatedBy, LastModified, LastModifiedBy),
    /// and excluding navigation/collection properties.
    /// </summary>
    /// <param name="source">The original property values to copy from.</param>
    /// <param name="target">The new property values to copy to.</param>
    public static void duplicateScalarValues(this EstatePropertyValues source, EstatePropertyValues target)
    {
        // General values
        target.Description = source.Description;
        target.AvailableFrom = DateTime.SpecifyKind(source.AvailableFrom, DateTimeKind.Utc);
        target.Capacity = source.Capacity;
        // Financial values
        target.Currency = source.Currency;
        target.SalePrice = source.SalePrice;
        target.RentPrice = source.RentPrice;
        target.HasCommonExpenses = source.HasCommonExpenses;
        target.CommonExpensesValue = source.CommonExpensesValue;
        // Features
        target.IsElectricityIncluded = source.IsElectricityIncluded;
        target.IsWaterIncluded = source.IsWaterIncluded;
        // Visibility
        target.IsPriceVisible = source.IsPriceVisible;
        target.Status = source.Status;
        target.IsActive = source.IsActive;
        target.IsPropertyVisible = source.IsPropertyVisible;
        target.IsFeatured = true;
    }
}

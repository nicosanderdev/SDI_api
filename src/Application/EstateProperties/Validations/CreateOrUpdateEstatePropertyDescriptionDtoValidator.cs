using SDI_Api.Application.EstateProperties.Commands;
using SDI_Api.Domain.Enums;

namespace SDI_Api.Application.EstateProperties.Validations;

using FluentValidation;

public class CreateOrUpdateEstatePropertyValueDtoValidator : AbstractValidator<CreateOrUpdateEstatePropertyValuesDto>
{
    public CreateOrUpdateEstatePropertyValueDtoValidator()
    {
        // --- ID Validations ---
        RuleFor(v => v.Id)
            .Must(BeAValidGuid)
            .When(v => !string.IsNullOrEmpty(v.Id))
            .WithMessage("'{PropertyName}' must be a valid identifier (GUID).");

        RuleFor(v => v.EstatePropertyId)
            .Must(BeAValidGuid)
            .When(v => !string.IsNullOrEmpty(v.EstatePropertyId))
            .WithMessage("'{PropertyName}' must be a valid identifier (GUID).");

        // --- Core Value Validations ---
        RuleFor(v => v.Description)
            .MaximumLength(2000).WithMessage("'{PropertyName}' cannot exceed 2000 characters.");

        RuleFor(v => v.Capacity)
            .GreaterThan(0).WithMessage("'{PropertyName}' must be at least 1.");

        RuleFor(v => v.AvailableFrom)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .When(v => v.AvailableFrom.HasValue)
            .WithMessage("'{PropertyName}' cannot be a date in the past.");

        RuleFor(v => v.Currency)
            .IsInEnum()
            .WithMessage("'{PropertyName}' has a range of values which does not include the supplied value.");

        RuleFor(v => v.PropertyStatus)
            .IsInEnum()
            .WithMessage("'{PropertyName}' has a range of values which does not include the supplied value.");


        // --- Pricing Validations ---
        // Prices cannot be negative.
        RuleFor(v => v.SalePrice)
            .GreaterThanOrEqualTo(0).WithMessage("'{PropertyName}' cannot be negative.");

        RuleFor(v => v.RentPrice)
            .GreaterThanOrEqualTo(0).WithMessage("'{PropertyName}' cannot be negative.");

        // Conditional pricing based on status.
        // If the property is for sale, a sale price must be provided.
        RuleFor(v => v.SalePrice)
            .GreaterThan(0)
            .When(v => v.PropertyStatus == PropertyStatus.Sale)
            .WithMessage("A '{PropertyName}' must be provided when the status is 'For Sale'.");

        // If the property is for rent, a rent price must be provided.
        RuleFor(v => v.RentPrice)
            .GreaterThan(0)
            .When(v => v.PropertyStatus == PropertyStatus.Rent)
            .WithMessage("A '{PropertyName}' must be provided when the status is 'For Rent'.");


        // --- Common Expenses Validations (Conditional Logic) ---
        // The amount for common expenses must be greater than zero ONLY if the 'HasCommonExpenses' flag is true.
        RuleFor(v => v.CommonExpensesAmount)
            .GreaterThan(0)
            .When(v => v.HasCommonExpenses)
            .WithMessage("'{PropertyName}' must be greater than zero when 'Has Common Expenses' is true.");

        // Conversely, the amount must be exactly zero if the 'HasCommonExpenses' flag is false.
        RuleFor(v => v.CommonExpensesAmount)
            .Equal(0m) // Use '0m' for decimal comparison
            .When(v => !v.HasCommonExpenses)
            .WithMessage("'{PropertyName}' must be zero when 'Has Common Expenses' is false.");
    }

    /// <summary>
    /// Helper method to check if a string can be parsed as a Guid.
    /// </summary>
    private bool BeAValidGuid(string? id)
    {
        return Guid.TryParse(id, out _);
    }
}


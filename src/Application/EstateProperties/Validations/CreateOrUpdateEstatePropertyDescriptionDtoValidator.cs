using SDI_Api.Application.EstateProperties.Commands;

namespace SDI_Api.Application.EstateProperties.Validations;

using FluentValidation;

public class CreateOrUpdateEstatePropertyDescriptionDtoValidator : AbstractValidator<CreateOrUpdateEstatePropertyDescriptionDto>
{
    public CreateOrUpdateEstatePropertyDescriptionDtoValidator()
    {
        RuleFor(x => x.EstatePropertyId)
            .NotEqual(String.Empty).WithMessage("EstatePropertyId should not be empty.");

        RuleFor(x => x.ListedPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Listed price must be 0 or greater.");

        RuleFor(x => x.RentPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Rent price must be 0 or greater.");

        RuleFor(x => x.SoldPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Sold price must be 0 or greater.");

        RuleFor(x => x.AvailableTo)
            .GreaterThan(x => x.AvailableFrom ?? DateTimeOffset.MinValue)
            .When(x => x.AvailableFrom.HasValue && x.AvailableTo.HasValue)
            .WithMessage("AvailableTo must be later than AvailableFrom.");
    }
}


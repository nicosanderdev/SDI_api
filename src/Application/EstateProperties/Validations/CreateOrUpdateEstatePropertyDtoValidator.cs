using SDI_Api.Application.Dtos;
using SDI_Api.Application.EstateProperties.Commands;
using Sdi_Api.Application.EstateProperties.Commands.Validations;

namespace SDI_Api.Application.EstateProperties.Validations;

public class CreateOrUpdateEstatePropertyDtoValidator : AbstractValidator<CreateOrUpdateEstatePropertyDto>
{
    public CreateOrUpdateEstatePropertyDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must be at most 100 characters.");

        RuleFor(x => x.RentPrice)
            .NotEmpty().WithMessage("Rent Price is required.");
        
        RuleFor(x => x)
            .Must(dto => dto.SalePrice > 0 || dto.RentPrice > 0)
            .WithMessage("Either a Sale Price or a Rent Price must be provided.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required.");

        RuleFor(x => x.Bedrooms)
            .GreaterThanOrEqualTo(0).WithMessage("Bedrooms must be 0 or more.");

        RuleFor(x => x.Bathrooms)
            .GreaterThanOrEqualTo(0).WithMessage("Bathrooms must be 0 or more.");

        RuleFor(x => x.ZipCode)
            .MaximumLength(10).WithMessage("ZipCode must be 10 characters or less.");

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country must be 100 characters or less.");
    }
}

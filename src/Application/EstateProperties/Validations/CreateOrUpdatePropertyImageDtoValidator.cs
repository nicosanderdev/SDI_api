using SDI_Api.Application.EstateProperties.Commands;

namespace Sdi_Api.Application.EstateProperties.Commands.Validations;

using FluentValidation;

public class CreateOrUpdatePropertyImageDtoValidator : AbstractValidator<CreateOrUpdatePropertyImageDto>
{
    public CreateOrUpdatePropertyImageDtoValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Image URL is required.")
            .MaximumLength(500).WithMessage("Image URL must be 500 characters or less.")
            .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute)).WithMessage("Image URL must be a valid URI.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .MaximumLength(255).WithMessage("File name must be 255 characters or less.");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required.")
            .MaximumLength(100).WithMessage("Content type must be 100 characters or less.");

        RuleFor(x => x.ImageData)
            .NotNull().WithMessage("Image data is required.")
            .Must(data => data.Length > 0).WithMessage("Image data cannot be empty.");

        RuleFor(x => x.EstatePropertyId)
            .NotEqual(Guid.Empty).WithMessage("EstatePropertyId must be a valid GUID.");

        // Optional: If AltText is provided, limit its length
        When(x => !string.IsNullOrWhiteSpace(x.AltText), () =>
        {
            RuleFor(x => x.AltText)
                .MaximumLength(200).WithMessage("Alt text must be 200 characters or less.");
        });
    }
}

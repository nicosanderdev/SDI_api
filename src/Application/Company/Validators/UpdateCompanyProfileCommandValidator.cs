using SDI_Api.Application.Company.Commands;

namespace SDI_Api.Application.Company.Validators;

public class UpdateCompanyProfileCommandValidator : AbstractValidator<UpdateCompanyProfileCommand>
{
    public UpdateCompanyProfileCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(v => v.ProfileData)
            .NotNull().WithMessage("Profile data is required.");

        When(v => v.ProfileData != null, () =>
        {
            RuleFor(v => v.ProfileData.Name)
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.")
                .When(v => !string.IsNullOrWhiteSpace(v.ProfileData.Name));

            RuleFor(v => v.ProfileData.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(v => !string.IsNullOrWhiteSpace(v.ProfileData.Description));

            RuleFor(v => v.ProfileData.Phone)
                .MaximumLength(50).WithMessage("Phone cannot exceed 50 characters.")
                .When(v => !string.IsNullOrWhiteSpace(v.ProfileData.Phone));

            When(v => v.ProfileData.Address != null, () =>
            {
                RuleFor(v => v.ProfileData.Address!.Street)
                    .MaximumLength(255).WithMessage("Street cannot exceed 255 characters.")
                    .When(v => !string.IsNullOrWhiteSpace(v.ProfileData.Address!.Street));

                RuleFor(v => v.ProfileData.Address!.Street2)
                    .MaximumLength(255).WithMessage("Street2 cannot exceed 255 characters.")
                    .When(v => !string.IsNullOrWhiteSpace(v.ProfileData.Address!.Street2));

                RuleFor(v => v.ProfileData.Address!.City)
                    .MaximumLength(100).WithMessage("City cannot exceed 100 characters.")
                    .When(v => !string.IsNullOrWhiteSpace(v.ProfileData.Address!.City));

                RuleFor(v => v.ProfileData.Address!.State)
                    .MaximumLength(100).WithMessage("State cannot exceed 100 characters.")
                    .When(v => !string.IsNullOrWhiteSpace(v.ProfileData.Address!.State));

                RuleFor(v => v.ProfileData.Address!.PostalCode)
                    .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters.")
                    .When(v => !string.IsNullOrWhiteSpace(v.ProfileData.Address!.PostalCode));

                RuleFor(v => v.ProfileData.Address!.Country)
                    .MaximumLength(100).WithMessage("Country cannot exceed 100 characters.")
                    .When(v => !string.IsNullOrWhiteSpace(v.ProfileData.Address!.Country));
            });
        });
    }
}


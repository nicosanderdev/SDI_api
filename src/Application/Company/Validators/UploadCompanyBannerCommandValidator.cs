using System.IO;
using SDI_Api.Application.Company.Commands;

namespace SDI_Api.Application.Company.Validators;

public class UploadCompanyBannerCommandValidator : AbstractValidator<UploadCompanyBannerCommand>
{
    public UploadCompanyBannerCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(v => v.BannerFile)
            .NotNull().WithMessage("Banner file is required.")
            .Must(file => file != null && file.Length > 0).WithMessage("Banner file cannot be empty.")
            .Must(file => file != null && file.Length <= 5 * 1024 * 1024).WithMessage("Banner file size cannot exceed 5MB.")
            .Must(file => file != null && IsValidImageExtension(file.FileName)).WithMessage("Banner file must be a valid image (jpg, jpeg, png, gif).");
    }

    private static bool IsValidImageExtension(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(extension);
    }
}


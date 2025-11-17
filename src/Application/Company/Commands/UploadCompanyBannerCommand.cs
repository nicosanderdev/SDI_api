using Microsoft.AspNetCore.Http;
using SDI_Api.Application.Common.Exceptions;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Company;
using SDI_Api.Domain.Entities;
using NotFoundException = SDI_Api.Application.Common.Exceptions.NotFoundException;

namespace SDI_Api.Application.Company.Commands;

public class UploadCompanyBannerCommand : IRequest<UploadCompanyImageResponseDto>
{
    public Guid? UserId { get; set; }
    public IFormFile BannerFile { get; set; } = null!;
}

public class UploadCompanyBannerCommandHandler : IRequestHandler<UploadCompanyBannerCommand, UploadCompanyImageResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public UploadCompanyBannerCommandHandler(
        IApplicationDbContext context,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    public async Task<UploadCompanyImageResponseDto> Handle(UploadCompanyBannerCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        if (userId == null || userId == Guid.Empty)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);
        if (member == null)
            throw new NotFoundException(nameof(Member), userId.ToString()!);

        var userCompany = await _context.UserCompanies
            .Where(uc => uc.MemberId == member.Id)
            .Include(uc => uc.Company)
            .FirstOrDefaultAsync(cancellationToken);

        if (userCompany == null)
            throw new NotFoundException("Company", "User is not associated with a company.");

        if (request.BannerFile == null || request.BannerFile.Length == 0)
            throw new ArgumentException("Banner file is required.", nameof(request.BannerFile));

        var company = userCompany.Company;

        // Delete old banner if exists
        if (!string.IsNullOrEmpty(company.BannerUrl))
            await _fileStorageService.DeleteFileAsync(company.BannerUrl);

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileResult = await _fileStorageService.SaveFileAsync(
            request.BannerFile,
            "StoragePaths:CompanyBanners",
            allowedExtensions,
            company.Id.ToString()
        );

        company.BannerUrl = fileResult.RelativePath;
        await _context.SaveChangesAsync(cancellationToken);

        return new UploadCompanyImageResponseDto { Url = fileResult.RelativePath };
    }
}


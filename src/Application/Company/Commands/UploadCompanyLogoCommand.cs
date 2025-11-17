using Microsoft.AspNetCore.Http;
using SDI_Api.Application.Common.Exceptions;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Company;
using SDI_Api.Domain.Entities;
using NotFoundException = SDI_Api.Application.Common.Exceptions.NotFoundException;

namespace SDI_Api.Application.Company.Commands;

public class UploadCompanyLogoCommand : IRequest<UploadCompanyImageResponseDto>
{
    public Guid? UserId { get; set; }
    public IFormFile LogoFile { get; set; } = null!;
}

public class UploadCompanyLogoCommandHandler : IRequestHandler<UploadCompanyLogoCommand, UploadCompanyImageResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public UploadCompanyLogoCommandHandler(
        IApplicationDbContext context,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    public async Task<UploadCompanyImageResponseDto> Handle(UploadCompanyLogoCommand request, CancellationToken cancellationToken)
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

        if (request.LogoFile == null || request.LogoFile.Length == 0)
            throw new ArgumentException("Logo file is required.", nameof(request.LogoFile));

        var company = userCompany.Company;

        // Delete old logo if exists
        if (!string.IsNullOrEmpty(company.LogoUrl))
            await _fileStorageService.DeleteFileAsync(company.LogoUrl);

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileResult = await _fileStorageService.SaveFileAsync(
            request.LogoFile,
            "StoragePaths:CompanyLogos",
            allowedExtensions,
            company.Id.ToString()
        );

        company.LogoUrl = fileResult.RelativePath;
        await _context.SaveChangesAsync(cancellationToken);

        return new UploadCompanyImageResponseDto { Url = fileResult.RelativePath };
    }
}


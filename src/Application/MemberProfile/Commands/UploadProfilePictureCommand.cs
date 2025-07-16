using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.MemberProfile.Commands;

public class UploadProfilePictureCommand : IRequest<UploadAvatarResponseDto>
{
    public Guid? UserId { get; set; }
    public IFormFile AvatarFile { get; set; } = null!;
}

public class UploadProfilePictureCommandHandler : IRequestHandler<UploadProfilePictureCommand, UploadAvatarResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public UploadProfilePictureCommandHandler(
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment,
        IApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    public async Task<UploadAvatarResponseDto> Handle(UploadProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);
        if (member == null)
            throw new NotFoundException(nameof(Member), userId.ToString()!);

        if (request.AvatarFile == null || request.AvatarFile.Length == 0)
            throw new ArgumentException("Avatar file is required.", nameof(request.AvatarFile));

        await _fileStorageService.DeleteFileAsync(member.AvatarUrl);
        
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileResult = await _fileStorageService.SaveFileAsync(
            request.AvatarFile,
            "StoragePaths:Avatars", // Key from appsettings.json
            allowedExtensions,
            request.UserId.ToString()! // Subfolder
        );
        
        member.AvatarUrl = fileResult.RelativePath;
        await _context.SaveChangesAsync(cancellationToken);
        
        return new UploadAvatarResponseDto { AvatarUrl = fileResult.RelativePath };
    }
}

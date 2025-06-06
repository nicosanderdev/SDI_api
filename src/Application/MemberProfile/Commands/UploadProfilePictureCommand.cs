using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.MemberProfile.Commands;

public class UploadProfilePictureCommand : IRequest<UploadAvatarResponseDto>
{
    public IFormFile AvatarFile { get; set; } = null!;
}

public class UploadProfilePictureCommandHandler : IRequestHandler<UploadProfilePictureCommand, UploadAvatarResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _configuration; // To get base path for avatars
    private readonly IWebHostEnvironment _webHostEnvironment; // To get wwwroot path
    private readonly IApplicationDbContext _context;

    public UploadProfilePictureCommandHandler(
        IIdentityService identityService,
        ICurrentUserService currentUserService,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment,
        IApplicationDbContext context)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
        _configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
        _context = context;
    }

    public async Task<UploadAvatarResponseDto> Handle(UploadProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);
        if (member == null)
        {
            throw new NotFoundException(nameof(Member), userId.ToString());
        }

        if (request.AvatarFile == null || request.AvatarFile.Length == 0)
        {
            throw new ArgumentException("Avatar file is required.", nameof(request.AvatarFile));
        }

        // Basic validation for image type (can be more robust)
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(request.AvatarFile.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Invalid file type. Only JPG, PNG, GIF are allowed.", nameof(request.AvatarFile));
        }
        
        // Define storage path. E.g., wwwroot/avatars/user_id/filename.ext
        // Ensure the 'Avatars' base path is configured or use a default.
        // string avatarBasePath = _configuration["StoragePaths:Avatars"] ?? "avatars"; // From appsettings.json
        string userAvatarFolder = Path.Combine("avatars", userId.ToString());
        string fullPathFolder = Path.Combine(_webHostEnvironment.WebRootPath, userAvatarFolder);

        if (!Directory.Exists(fullPathFolder))
        {
            Directory.CreateDirectory(fullPathFolder);
        }

        // Delete old avatar if exists and filename changes or if you want to ensure only one avatar
        if (!string.IsNullOrEmpty(member.AvatarUrl))
        {
            var oldFileName = Path.GetFileName(new Uri(member.AvatarUrl, UriKind.RelativeOrAbsolute).LocalPath);
            var oldFilePath = Path.Combine(fullPathFolder, oldFileName);
            if (File.Exists(oldFilePath) && Path.GetFileName(request.AvatarFile.FileName) != oldFileName) // Simple check
            {
                try { File.Delete(oldFilePath); } catch {/* Log error but continue */}
            }
        }


        var uniqueFileName = Guid.NewGuid().ToString() + extension; // Ensure unique filename
        var filePath = Path.Combine(fullPathFolder, uniqueFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.AvatarFile.CopyToAsync(stream, cancellationToken);
        }
        
        // Assuming they are served directly from /
        var publicAvatarUrl = $"/{userAvatarFolder.Replace("\\", "/")}/{uniqueFileName}";

        member.AvatarUrl = publicAvatarUrl;
        try
        {
            var updateResult = await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException e)
        {
            if (File.Exists(filePath)) { File.Delete(filePath); }
            var errors = string.Join(", ", e.Message);
            throw new Exception($"Avatar update failed: {errors}");
        }
        
        return new UploadAvatarResponseDto { AvatarUrl = publicAvatarUrl };
    }
}

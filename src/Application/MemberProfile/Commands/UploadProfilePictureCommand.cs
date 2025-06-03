using Microsoft.Extensions.Configuration;

namespace SDI_Api.Application.MemberProfile.Commands;

public class UploadProfilePictureCommand : IRequest<UploadAvatarResponseDto>
{
    public IFormFile AvatarFile { get; set; } = null!;
}

public class UploadProfilePictureCommandHandler : IRequestHandler<UploadProfilePictureCommand, UploadAvatarResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _configuration; // To get base path for avatars
    private readonly IWebHostEnvironment _webHostEnvironment; // To get wwwroot path

    public UploadProfilePictureCommandHandler(
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
        _configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<UploadAvatarResponseDto> Handle(UploadProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserIdGuid();
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
        {
            throw new NotFoundException(nameof(ApplicationUser), userId.Value);
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
        string userAvatarFolder = Path.Combine("avatars", userId.Value.ToString());
        string fullPathFolder = Path.Combine(_webHostEnvironment.WebRootPath, userAvatarFolder);

        if (!Directory.Exists(fullPathFolder))
        {
            Directory.CreateDirectory(fullPathFolder);
        }

        // Delete old avatar if exists and filename changes or if you want to ensure only one avatar
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            var oldFileName = Path.GetFileName(new Uri(user.AvatarUrl, UriKind.RelativeOrAbsolute).LocalPath);
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

        // Construct the public URL. This depends on how wwwroot files are served.
        // Assuming they are served directly from /
        var publicAvatarUrl = $"/{userAvatarFolder.Replace("\\", "/")}/{uniqueFileName}";

        user.AvatarUrl = publicAvatarUrl;
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            // Clean up saved file if DB update fails
            if (File.Exists(filePath)) try { File.Delete(filePath); } catch {/* Log error */}
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            throw new Exception($"Avatar update failed: {errors}");
        }

        return new UploadAvatarResponseDto { AvatarUrl = publicAvatarUrl };
    }
}

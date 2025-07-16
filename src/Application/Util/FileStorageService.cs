using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Common;

namespace SDI_Api.Application.Util;


public class FileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FileStorageService(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<FileSaveResult> SaveFileAsync(IFormFile file, string rootPathConfigKey, string[] allowedExtensions, params string[] subfolders)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is required.", nameof(file));

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException($"Invalid file type. Only {string.Join(", ", allowedExtensions)} are allowed.", nameof(file));

        var rootPathFromConfig = _configuration[rootPathConfigKey];
        if (string.IsNullOrEmpty(rootPathFromConfig))
            throw new InvalidOperationException($"Configuration for '{rootPathConfigKey}' is missing.");

        // Combine all parts of the path: wwwroot + rootFromConfig + subfolders
        var pathParts = new[] { _webHostEnvironment.WebRootPath, rootPathFromConfig }.Concat(subfolders).ToArray();
        var fullFolderPath = Path.Combine(pathParts);

        Directory.CreateDirectory(fullFolderPath);

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var fullFilePath = Path.Combine(fullFolderPath, uniqueFileName);

        await using (var stream = new FileStream(fullFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Create the relative path for database storage
        var relativePath = Path.Combine(new[] { "/", rootPathFromConfig }.Concat(subfolders).Concat(new[] { uniqueFileName }).ToArray())
                               .Replace(Path.DirectorySeparatorChar, '/');
        // Create the full public URL for the client response
        var request = _httpContextAccessor.HttpContext!.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        var publicUrl = $"{baseUrl}{relativePath}";

        return new FileSaveResult(relativePath, publicUrl, file.FileName, file.ContentType);
    }

    public Task DeleteFileAsync(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return Task.CompletedTask;
        var fullFilePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/'));
        if (File.Exists(fullFilePath))
            try { File.Delete(fullFilePath); } catch (IOException ex) { Console.WriteLine($"Error deleting file {fullFilePath}: {ex.Message}"); }
        return Task.CompletedTask;
    }
}

using Microsoft.AspNetCore.Http;
using SDI_Api.Application.DTOs.Common;

namespace SDI_Api.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to a specified path and returns the result.
    /// </summary>
    /// <param name="file">The IFormFile to save.</param>
    /// <param name="rootPathConfigKey">The key in appsettings.json for the root storage folder (e.g., "StoragePaths:Avatars").</param>
    /// <param name="allowedExtensions">An array of allowed file extensions (e.g., [".jpg", ".png"]).</param>
    /// <param name="subfolders">An array of subfolders to create within the root path (e.g., ["userId", "documents"]).</param>
    /// <returns>A FileSaveResult containing paths and metadata.</returns>
    Task<FileSaveResult> SaveFileAsync(IFormFile file, string rootPathConfigKey, string[] allowedExtensions, params string[] subfolders);

    /// <summary>
    /// Deletes a file based on its relative path.
    /// </summary>
    /// <param name="relativePath">The relative path of the file to delete, as stored in the database.</param>
    Task DeleteFileAsync(string? relativePath);
}

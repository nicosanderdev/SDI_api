namespace SDI_Api.Application.DTOs.Common;

public record FileSaveResult(
    string RelativePath, // The path to store in the database (e.g., /uploads/avatars/...)
    string PublicUrl,    // The full public URL to return to a client
    string FileName,     // The original file name
    string ContentType
);

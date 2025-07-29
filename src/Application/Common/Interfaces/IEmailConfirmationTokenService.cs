using SDI_Api.Application.DTOs.Email;

namespace SDI_Api.Application.Common.Interfaces;

public interface IEmailConfirmationTokenService
{
    string GenerateToken(string userId, string userEmail);
    TokenValidationResult ValidateToken(string userEmail, string token);
}

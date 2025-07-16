using SDI_Api.Application.DTOs.Email;

namespace SDI_Api.Application.Common.Interfaces;

public interface IEmailConfirmationTokenService
{
    string GenerateToken(string userId);
    TokenValidationResult ValidateToken(string token);
}

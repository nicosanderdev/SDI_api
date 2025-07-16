
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SDI_Api.Application.Common.Interfaces;
using TokenValidationResult = SDI_Api.Application.DTOs.Email.TokenValidationResult;

namespace SDI_Api.Application.Util.Emails;

public class EmailConfirmationTokenService : IEmailConfirmationTokenService
{
    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly string _audience;
    private const string TokenPurpose = "email_confirmation";

    public EmailConfirmationTokenService(IConfiguration configuration)
    {
        var secretKey = Environment.GetEnvironmentVariable("EMAIL_CONFIRMATION_SECRET");
        _issuer = configuration["Jwt:Issuer"]!;
        _audience = configuration["Jwt:Audience"]!;

        if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
            throw new InvalidOperationException("JWT Key is not configured or is too short. It must be at least 32 characters long.");

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }

    public string GenerateToken(string userId)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("purpose", TokenPurpose)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public TokenValidationResult ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,

            ValidateIssuer = true,
            ValidIssuer = _issuer,

            ValidateAudience = true,
            ValidAudience = _audience,
            
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            // Validate the token and get the claims principal
            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            
            // **CRITICAL: Check that the purpose claim is correct**
            if (!principal.HasClaim(c => c.Type == "purpose" && c.Value == TokenPurpose))
                return new TokenValidationResult { IsValid = false, ErrorMessage = "Invalid token purpose." };

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return new TokenValidationResult { IsValid = false, ErrorMessage = "User ID not found in token." };

            return new TokenValidationResult { IsValid = true, UserId = userId };
        }
        catch (SecurityTokenException ex)
        {
            return new TokenValidationResult { IsValid = false, ErrorMessage = ex.Message };
        }
        catch (Exception ex)
        {
            return new TokenValidationResult { IsValid = false, ErrorMessage = ex + " An unexpected error occurred during token validation." };
        }
    }
}

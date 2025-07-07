namespace SDI_Api.Application.DTOs.Auth;

/// <summary>
/// Represents the data returned when generating a new 2FA authenticator key.
/// </summary>
public class GenerateTwoFactorKeyResponse
{
    /// <summary>
    /// The secret key that users can manually enter into their authenticator app.
    /// </summary>
    public string SharedKey { get; set; } = string.Empty;

    /// <summary>
    /// The URI containing all the necessary information to generate a QR code.
    /// </summary>
    public string AuthenticatorUri { get; set; } = string.Empty;
}

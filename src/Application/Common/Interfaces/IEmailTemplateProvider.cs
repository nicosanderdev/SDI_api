namespace SDI_Api.Application.Common.Interfaces;

public interface IEmailTemplateProvider
{
    /// <summary>
    /// Generates the HTML body for an email confirmation link.
    /// </summary>
    string GetConfirmationEmailBody(string confirmationLink);

    /// <summary>
    /// Generates the HTML body for a password reset link.
    /// </summary>
    string GetPasswordResetBody(string resetLink);

    /// <summary>
    /// Generates the HTML body for sending a 2FA token.
    /// </summary>
    string GetTwoFactorCodeBody(string twoFactorCode);

    /// <summary>
    /// Generates the HTML body for a link to enable 2FA.
    /// </summary>
    string GetEnableTwoFactorAuthBody(string enableLink);
}

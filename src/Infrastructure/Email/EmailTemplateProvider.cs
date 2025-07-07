using Microsoft.Extensions.Options;
using SDI_Api.Application.Common.Interfaces;

namespace SDI_Api.Infrastructure.Email;

public class EmailTemplateProvider : IEmailTemplateProvider
    {
        private readonly WebAppOptions _webAppOptions;

        public EmailTemplateProvider(IOptions<WebAppOptions> webAppOptions)
        {
            _webAppOptions = webAppOptions.Value;
        }

        public string GetConfirmationEmailBody(string confirmationLink)
        {
            var title = "Confirm Your Email Address";
            var bodyContent = $@"
                <p>Thank you for registering with us! Please confirm your email address to complete your registration by clicking the button below:</p>
                <a href='{confirmationLink}' class='button'>Confirm Email Address</a>
                <p>If you did not register on our site, you can safely ignore this email.</p>";

            return GetHtmlLayout(title, bodyContent);
        }

        public string GetPasswordResetBody(string resetLink)
        {
            var title = "Reset Your Password";
            var bodyContent = $@"
                <p>We received a request to reset the password for your account. You can reset your password by clicking the button below:</p>
                <a href='{resetLink}' class='button'>Reset Password</a>
                <p>If you did not request a password reset, please ignore this email. This link is valid for a limited time.</p>";
            
            return GetHtmlLayout(title, bodyContent);
        }

        public string GetTwoFactorCodeBody(string twoFactorCode)
        {
            var title = "Your Two-Factor Code";
            var bodyContent = $@"
                <p>Here is your two-factor authentication code. Please use it to complete your sign-in.</p>
                <p class='code'>{twoFactorCode}</p>
                <p>This code will expire shortly. If you did not request this code, please secure your account immediately.</p>";

            return GetHtmlLayout(title, bodyContent);
        }

        public string GetEnableTwoFactorAuthBody(string enableLink)
        {
            var title = "Activate Two-Factor Authentication";
            var bodyContent = $@"
                <p>A request was made to enable two-factor authentication (2FA) for your account. To complete the setup, please click the button below:</p>
                <a href='{enableLink}' class='button'>Activate 2FA</a>
                <p>If you did not make this request, you can safely ignore this email.</p>";

            return GetHtmlLayout(title, bodyContent);
        }

        /// <summary>
        /// A private helper to wrap content in a consistent HTML layout with basic styling.
        /// </summary>
        private string GetHtmlLayout(string title, string bodyContent)
        {
            // Using string.Replace for simplicity. For more complex scenarios, consider a templating engine like Razor or Scriban.
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>{title}</title>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ width: 100%; max-width: 600px; margin: 20px auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
                    .header {{ font-size: 24px; font-weight: bold; text-align: center; margin-bottom: 20px; color: #0056b3; }}
                    .content {{ font-size: 16px; }}
                    .footer {{ margin-top: 20px; font-size: 12px; color: #777; text-align: center; }}
                    .button {{
                        display: inline-block;
                        padding: 10px 20px;
                        margin: 20px 0;
                        background-color: #007bff;
                        color: #ffffff;
                        text-decoration: none;
                        border-radius: 5px;
                        font-weight: bold;
                    }}
                    .code {{
                        font-size: 22px;
                        font-weight: bold;
                        letter-spacing: 3px;
                        text-align: center;
                        padding: 15px;
                        background-color: #f2f2f2;
                        border-radius: 5px;
                        margin: 20px 0;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>{title}</div>
                    <div class='content'>
                        {bodyContent}
                        <p>Thanks,<br>The {_webAppOptions.Name} Team</p>
                    </div>
                    <div class='footer'>
                        © {DateTime.UtcNow.Year} {_webAppOptions.Name}. All rights reserved.
                    </div>
                </div>
            </body>
            </html>";
        }
    }

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models; // Assuming you have a Result class here
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SDI_Api.Infrastructure.Email;

/// <summary>
/// Configuration options for the email service.
/// These values should be loaded from appsettings.json or environment variables.
/// </summary>
public class EmailOptions
{
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    // If you prefer to configure the SendGrid API key via appsettings.json
    // instead of an environment variable, you can add it here:
    // public string SendGridApiKey { get; set; } = string.Empty;
}

public class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Sends an email asynchronously using SendGrid.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The content of the email.</param>
    /// <param name="isHtml">True if the body is HTML content, false for plain text.</param>
    /// <returns>A <see cref="Result"/> indicating the success or failure of the email sending operation.</returns>
    public async Task<Result> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
    {
        var emailServiceFlag = Environment.GetEnvironmentVariable("EMAIL_SERVICE_ENABLED");
        if (string.IsNullOrEmpty(emailServiceFlag))
        {
            _logger.LogError("SendGrid API Key is not configured. Email cannot be sent.");
            return Result.Failure(new[] { "Email service is not configured: Is service enabled flag missing." });
        }

        if (!bool.Parse(emailServiceFlag))
        {
            _logger.LogInformation("Logging email info as Email Service is disabled");
            _logger.LogInformation(toEmail + " " + subject + " " + body + " " + isHtml );
            return Result.Success();
        }
        
        try
        {
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("SendGrid API Key is not configured. Email cannot be sent.");
                return Result.Failure(new[] { "Email service is not configured: SendGrid API Key is missing." });
            }

            if (string.IsNullOrEmpty(_options.FromEmail))
            {
                _logger.LogError("EmailOptions.FromEmail is not configured. Email cannot be sent.");
                return Result.Failure(new[] { "Email service is not configured: Sender email address is missing." });
            }

            var client = new SendGridClient(apiKey);
            var fromEmailAddress = new EmailAddress(_options.FromEmail, _options.FromName);
            var toEmailAddress = new EmailAddress(toEmail);

            var msg = new SendGridMessage
            {
                From = fromEmailAddress,
                Subject = subject
            };

            if (isHtml)
            {
                msg.HtmlContent = body;
                msg.PlainTextContent = string.Empty;
            }
            else
            {
                msg.PlainTextContent = body;
                msg.HtmlContent = string.Empty;
            }

            msg.AddTo(toEmailAddress);
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {ToEmail} with subject '{Subject}'. Status Code: {StatusCode}",
                    toEmail, subject, response.StatusCode);
                return Result.Success();
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send email to {ToEmail} with subject '{Subject}'. Status Code: {StatusCode}. Response Body: {ResponseBody}",
                    toEmail, subject, response.StatusCode, responseBody);
                return Result.Failure(new[] { $"Failed to send email. Status: {response.StatusCode}. Details: {responseBody}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while sending email to {ToEmail} with subject '{Subject}'.", toEmail, subject);
            return Result.Failure(new[] { $"An unexpected error occurred while sending email: {ex.Message}" });
        }
    }
}

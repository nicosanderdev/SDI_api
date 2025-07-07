using SDI_Api.Application.Common.Models; // Assuming Result is here

namespace SDI_Api.Application.Common.Interfaces;

public interface IEmailService
{
    Task<Result> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
}

using Microsoft.Extensions.Configuration;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;

namespace SDI_Api.Application.Auth.Commands;

/// <summary>
/// Command to generate and send a new email confirmation link.
/// </summary>
public record ResendConfirmationEmailCommand(string Email) : IRequest<Result>;

/// <summary>
/// Handles the logic for the ResendConfirmationEmailCommand.
/// </summary>
public class ResendConfirmationEmailCommandHandler : IRequestHandler<ResendConfirmationEmailCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateProvider _emailTemplateProvider; // Service to generate frontend URLs
    private readonly IConfiguration _configuration;

    public ResendConfirmationEmailCommandHandler(IIdentityService identityService, IEmailService emailService, IEmailTemplateProvider emailTemplateProvider, IConfiguration configuration)
    {
        _identityService = identityService;
        _emailService = emailService;
        _emailTemplateProvider = emailTemplateProvider;
        _configuration = configuration;
    }

    public async Task<Result> Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByEmailAsync(request.Email);

        // Only generate and send an email if the user exists and their email is not yet confirmed.
        // This check happens silently to prevent attackers from discovering which emails are registered or confirmed.
        if (user != null && !user.isEmailConfirmed())
        {
            var token = await _identityService.GenerateEmailConfirmationTokenAsync(user);

            var confirmationLink = _configuration["AppUrls:ReactAppConfirmationUrl"];
            if (string.IsNullOrEmpty(confirmationLink))
                throw new InvalidOperationException("Confirmation Link is not configured in appsettings.json.");
            
            var emailBody = _emailTemplateProvider.GetConfirmationEmailBody(confirmationLink);
            await _emailService.SendEmailAsync(
                toEmail: user.getUserEmail()!,
                subject: "Confirm Your Email Address",
                body: emailBody,
                true);
        }
        return Result.Success();
    }
}

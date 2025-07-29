using Microsoft.Extensions.Configuration;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;

namespace SDI_Api.Application.Auth.Commands;

/// <summary>
/// Command to generate and send a new email confirmation link.
/// </summary>
public record ResendConfirmationEmailCommand() : IRequest<Result>
{
    public string? UserId { get; set; }
}

/// <summary>
/// Handles the logic for the ResendConfirmationEmailCommand.
/// </summary>
public class ResendConfirmationEmailCommandHandler : IRequestHandler<ResendConfirmationEmailCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateProvider _emailTemplateProvider; // Service to generate frontend URLs
    private readonly IConfiguration _configuration;
    private readonly IEmailConfirmationTokenService _tokenService;

    public ResendConfirmationEmailCommandHandler(IIdentityService identityService, IEmailService emailService, IEmailTemplateProvider emailTemplateProvider, IConfiguration configuration, IEmailConfirmationTokenService tokenService)
    {
        _identityService = identityService;
        _emailService = emailService;
        _emailTemplateProvider = emailTemplateProvider;
        _configuration = configuration;
        _tokenService = tokenService;
    }

    public async Task<Result> Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(request.UserId!);
        if (user != null && !user.isEmailConfirmed())
        {
            // Send email for confirmation
            var confirmationLink = _configuration["AppUrls:ReactAppConfirmationUrl"];
            var token = _tokenService.GenerateToken(user.getId()!, user.getUserEmail()!);
            if (string.IsNullOrEmpty(confirmationLink))
                throw new InvalidOperationException("Confirmation Link is not configured in appsettings.json.");
            
            var emailBody = _emailTemplateProvider.GetConfirmationEmailBody(confirmationLink+$"?token={token}");
            await _emailService.SendEmailAsync(
                toEmail: user.getUserEmail()!,
                subject: "Confirm Your Email Address",
                body: emailBody,
                true);
        }
        return Result.Success();
    }
}

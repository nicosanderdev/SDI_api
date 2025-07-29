using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.Extensions.Configuration;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.DTOs.Auth;

namespace SDI_Api.Application.Auth.Commands;

/// <summary>
/// Command to reset a user's password using a token.
/// This record is also used as the DTO for the API request body.
/// </summary>
public record ResetPasswordCommand : IRequest<Result>
{
    public string? Email { get; set; }
    public string? Token { get; set; }
    public string? NewPassword { get; set; }
    public bool ResetEmail { get; set; }
}

/// <summary>
/// Handles the logic for the ResetPasswordCommand.
/// </summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateProvider _emailTemplateProvider;
    private readonly IConfiguration _configuration;
    private readonly IEmailConfirmationTokenService _tokenService;

    public ResetPasswordCommandHandler(IIdentityService identityService, IEmailService emailService, IEmailTemplateProvider emailTemplateProvider, IConfiguration configuration, IEmailConfirmationTokenService tokenService)
    {
        _identityService = identityService;
        _emailService = emailService;
        _emailTemplateProvider = emailTemplateProvider;
        _configuration = configuration;
        _tokenService = tokenService;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // var request = command.ResetPasswordDto!;
        var user = await _identityService.FindUserByEmailAsync(request.Email!);
        if (user is null)
            return Result.Failure(new[] { "Invalid token or email." });

        if (request.ResetEmail)
        {
            var emailResult = await _identityService.SetEmailAsync(user.getId()!, request.Email!);
            
            var confirmationLink = _configuration["AppUrls:ReactAppConfirmationUrl"];
            var token = _tokenService.GenerateToken(user.getId()!, request.Email!);
            if (string.IsNullOrEmpty(confirmationLink))
                throw new InvalidOperationException("Confirmation Link is not configured in appsettings.json.");
            
            var emailBody = _emailTemplateProvider.GetConfirmationEmailBody(confirmationLink+$"?token={token}");
            await _emailService.SendEmailAsync(
                toEmail: request.Email!,
                subject: "Confirm Your Email Address",
                body: emailBody,
                true);
        }
        
        var result = await _identityService.ResetPasswordAsync(user.getId()!, request.Token!, request.NewPassword!);
        return result;
    }
}

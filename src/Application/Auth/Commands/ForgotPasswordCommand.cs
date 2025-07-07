using System.Text.Encodings.Web;
using Microsoft.Extensions.Configuration;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;

namespace SDI_Api.Application.Auth.Commands;

public class ForgotPasswordCommand : IRequest<Result>
{
    public string? Email { get; }
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IEmailTemplateProvider _emailTemplateProvider;

    public ForgotPasswordCommandHandler(
        IIdentityService identityService,
        IEmailService emailService, 
        IConfiguration configuration,
        IEmailTemplateProvider emailTemplateProvider)
    {
        _identityService = identityService;
        _emailService = emailService;
        _configuration = configuration;
        _emailTemplateProvider = emailTemplateProvider;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByEmailAsync(request.Email!);
        if (user == null)
            throw new Exception("User was not found");
        
        var token = await _identityService.GeneratePasswordResetTokenAsync(user);
        
        var reactAppResetUrl = _configuration["AppUrls:ReactAppResetPasswordUrl"];
        if (string.IsNullOrEmpty(reactAppResetUrl))
            throw new InvalidOperationException("App Reset Password URL is not configured in appsettings.json.");
        
        var encodedToken = UrlEncoder.Default.Encode(token);
        var callbackUrl = $"{reactAppResetUrl}?token={encodedToken}&email={user.getUserEmail()}";
        
        var emailSubject = "Reset Your Password";
        var emailBody = _emailTemplateProvider.GetPasswordResetBody(callbackUrl);

        await _emailService.SendEmailAsync(user.getUserEmail()!, emailSubject, emailBody);
        return Result.Success();
    }
}

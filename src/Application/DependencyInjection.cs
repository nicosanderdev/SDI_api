using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SDI_Api.Application.Common.Behaviours;
using Microsoft.Extensions.Hosting;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Security;
using SDI_Api.Application.Util;
using SDI_Api.Application.Util.Emails;
using SDI_Api.Application.Util.RecoveryCodes;

namespace SDI_Api.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddSingleton<IRecoveryCodeGenerator, RecoveryCodeGenerator>();
        builder.Services.AddSingleton<IEmailConfirmationTokenService, EmailConfirmationTokenService>();
        
        builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Services.AddScoped<IFileStorageService, FileStorageService>();

        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        });
    }
}

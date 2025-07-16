using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Infrastructure.Data;
using Sdi_Api.Infrastructure.Email;
using SDI_Api.Infrastructure.Email;
using SDI_Api.Web.Services;

namespace SDI_Api.Web;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        builder.Services.AddExceptionHandler<CustomExceptionHandler>();

        builder.Services.AddControllers();
        
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddTransient<IFormFile>(sp =>
        {
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            var httpContext = httpContextAccessor.HttpContext;
            
            if (httpContext?.Request.Form.Files.Any() ?? false)
                return httpContext.Request.Form.Files[0];
            
            return null!;
        });
        
        builder.Services.AddOpenApiDocument((configure, sp) =>
        {
            configure.Title = "SDI_Api API";
        });
    }

    public static void AddKeyVaultIfConfigured(this IHostApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }
    }
}

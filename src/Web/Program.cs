using SDI_Api.Application;
using SDI_Api.Infrastructure;
using SDI_Api.Infrastructure.Identity;
using SDI_Api.Web;
using SDI_Api.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
    app.MapSwagger();
}
else
{
    app.UseHsts();
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapFallbackToFile("index.html");

app.UseExceptionHandler(options => { });

app.MapIdentityApi<ApplicationUser>();
app.MapControllers();

app.UseRouting();
app.UseCors("FrontendCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.Run();

public partial class Program { }

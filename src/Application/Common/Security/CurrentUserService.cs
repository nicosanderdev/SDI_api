using Microsoft.AspNetCore.Http;
using SDI_Api.Application.Common.Interfaces;

namespace SDI_Api.Application.Common.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst("id")?.Value;
    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetUserId()
    {
        return UserId != null ? Guid.Parse(UserId) : Guid.Empty;
    }
}

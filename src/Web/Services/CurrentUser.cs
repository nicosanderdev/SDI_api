using System.Security.Claims;

using SDI_Api.Application.Common.Interfaces;

namespace SDI_Api.Web.Services;

public class CurrentUser 
    //: IUser
{
    /* private readonly HttpContextAccessor _httpContextAccessor;

    public CurrentUser(HttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private string? Id => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? getId() => Id;

    public string? getUsername() => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public string? getUserEmail() => getUsername();

    public string? getPhoneNumber() => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.MobilePhone) 
                                        ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.HomePhone);*/
}

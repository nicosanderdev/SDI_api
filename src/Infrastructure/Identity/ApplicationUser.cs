using Microsoft.AspNetCore.Identity;
using SDI_Api.Application.Common.Interfaces;

namespace SDI_Api.Infrastructure.Identity;

public class ApplicationUser : IdentityUser, IUser
{
    public string? getId()
    {
        return Id;
    }

    public string? getUsername()
    {
        return UserName;
    }

    public string? getUserEmail()
    {
        return Email;
    }
    
    public string? getPhoneNumber()
    {
        return PhoneNumber;
    }

    public bool isEmailConfirmed()
    {
        return EmailConfirmed;
    }

    public bool isTwoFactorEnabled()
    {
        return TwoFactorEnabled;
    }
}

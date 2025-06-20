namespace SDI_Api.Application.Common.Interfaces;

public interface IUser
{
    string? getId();

    string? getUsername();

    string? getUserEmail();
    
    string? getPhoneNumber();
    
    bool isEmailConfirmed();
}

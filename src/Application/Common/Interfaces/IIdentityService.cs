using SDI_Api.Application.Common.Models;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Common.Interfaces;

public interface IIdentityService
{
    // --- ApplicationUser Management ---
    Task<string?> GetUserNameAsync(string userId);
    Task<IUser?> FindUserByIdAsync(string userId);
    Task<IUser?> FindUserByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(IUser user, string password);
    Task<(Result Result, string UserId)> CreateUserAsync(string email, string password, string? firstName, string? lastName, CancellationToken cancellationToken); // Modified to include names for Member creation
    Task<Result> DeleteUserAsync(string userId);
    // --- Email & Phone ---
    Task<Result> SetEmailAsync(string userId, string newEmail);
    Task<string> GenerateEmailConfirmationTokenAsync(IUser user);
    Task<Result> ConfirmEmailAsync(string userId, string token);
    Task<Result> SetPhoneNumberAsync(string userId, string? phoneNumber);
    // Add phone confirmation methods if needed
    // --- Password Management ---
    Task<Result> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
    Task<Result> ResetPasswordAsync(string userId, string token, string newPassword);
    Task<string> GeneratePasswordResetTokenAsync(IUser user);
    // --- Role & Policy Management ---
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<Result> AddToRoleAsync(string userId, string role);
    Task<Result> RemoveFromRoleAsync(string userId, string role);
    Task<IList<string>> GetUserRolesAsync(string userId);
    Task<bool> AuthorizeAsync(string userId, string policyName);
}

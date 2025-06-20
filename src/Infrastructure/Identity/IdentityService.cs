using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SDI_Api.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly IApplicationDbContext _context;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        IApplicationDbContext context,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _context = context;
        _signInManager = signInManager;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.getUsername();
    }

    public async Task<IUser?> FindUserByIdAsync(string userId)
    {
        var user = await _userManager.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null)
        {
            return null;
        }

        return user;
    }

    public async Task<IUser?> FindUserByEmailAsync(string email)
    {
        var user = await _userManager.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
        if (user == null)
        {
            return null;
        }

        return user;
    }
    
    public async Task<IUser?> FindUserByUsernameAsync(string username)
    {
        var user = await _userManager.Users.Where(u => u.UserName == username).FirstOrDefaultAsync();
        if (user == null)
        {
            return null;
        }

        return user;
    }

    public async Task<bool> GetTwoFactorEnabledAsync(IUser user)
    {
        var userId = user.getId();
        var userBD = await _userManager.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
        if (userBD == null)
        {
            return false;
        }

        return userBD.TwoFactorEnabled;
    }
    
    public async Task<bool> CheckPasswordAsync(IUser user, string password)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(user.getUserEmail()))
        {
            return false;
        }
        var existingUser = await _userManager.FindByEmailAsync(user.getUserEmail()!);
        if (existingUser == null)
        {
            return false;
        }
        return await _userManager.CheckPasswordAsync(existingUser, password);
    }

    public async Task<SignInResult> CheckPasswordSignInAsync(IUser? user, string password, bool lockoutOnFailure)
    {
        // TODO - revisar lógica
        var userDb = user == null ? null : await _userManager.FindByIdAsync(user.getId()!);
        if (userDb == null)
        {
            return SignInResult.Failed;
        }
        return await _signInManager.CheckPasswordSignInAsync(userDb, password, lockoutOnFailure);
    }

    public async Task<SignInResult> PasswordSignInAsync(IUser user, string password, bool isPersistent, bool lockoutOnFailure)
    {
        // TODO - revisar lógica
        var userDb =  await _userManager.FindByIdAsync(user.getId()!);
        if (userDb == null)
        {
            return SignInResult.Failed;
        }

        return await _signInManager.PasswordSignInAsync(userDb, password, isPersistent, lockoutOnFailure);
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string email, string password, string? firstName, string? lastName, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return (Result.Failure(["User with this email already exists."]), string.Empty);
        }

        var user = new ApplicationUser()
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            Email = email,
            EmailConfirmed = false
        };

        var identityResult = await _userManager.CreateAsync(user, password);
        return !identityResult.Succeeded ? (identityResult.ToApplicationResult(), string.Empty) :
            //    await _userManager.DeleteAsync(user); // Best effort cleanup
            //    return (Result.Failure(new[] { "Failed to create user profile.", ex.Message }), string.Empty);
            (Result.Success(), user.getId()!);
    }

    public Task<Result> DeleteUserAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> DeleteUserAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Success();
        }
        var identityResult = await _userManager.DeleteAsync(user);
        return identityResult.ToApplicationResult();
    }


    // --- Email & Phone ---
    public async Task<Result> SetEmailAsync(string userId, string newEmail)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) 
            return Result.Failure(["User not found."]);

        // Check if new email is already in use by another user
        var existingUserWithNewEmail = await _userManager.FindByEmailAsync(newEmail);
        if (existingUserWithNewEmail != null && existingUserWithNewEmail.getId() != userId)
        {
            return Result.Failure(["This email address is already in use."]);
        }
        
        // Note: UserManager.SetEmailAsync also sets EmailConfirmed to false.
        // You might need to handle re-confirmation.
        var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
        var result = await _userManager.ChangeEmailAsync(user, newEmail, token);

        if (result.Succeeded)
        {
            // If username is also email, update username
            var userNameResult = await _userManager.SetUserNameAsync(user, newEmail);
            return userNameResult.ToApplicationResult();
        }
        return result.ToApplicationResult();
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(IUser user)
    {
        if (user.getId() == null) 
            return string.Empty;
        var userDb = await _userManager.FindByIdAsync(user.getId()!);
        if (userDb == null) 
            return Result.Failure(["User not found."]).ToString()!;
        return await _userManager.GenerateEmailConfirmationTokenAsync(userDb);
    }

    public async Task<Result> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Result.Failure(["User not found."]);
        
        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.ToApplicationResult();
    }

    public async Task<Result> SetPhoneNumberAsync(string userId, string? phoneNumber)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Result.Failure(["User not found."]);
        
        // Similarly, SetPhoneNumberAsync sets PhoneNumberConfirmed to false.
        var result = await _userManager.SetPhoneNumberAsync(user, phoneNumber);
        return result.ToApplicationResult();
    }

    // --- Password Management ---
    public async Task<Result> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Result.Failure(["User not found."]);

        var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        return result.ToApplicationResult();
    }
    
    public async Task<string> GeneratePasswordResetTokenAsync(IUser user)
    {
        var userDb = await _userManager.FindByIdAsync(user.getId()!);
        if (userDb == null) return Result.Failure(["User not found."]).ToString()!;
        
        return await _userManager.GeneratePasswordResetTokenAsync(userDb);
    }

    public async Task<Result> ResetPasswordAsync(string userId, string token, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Result.Failure(["User not found."]);

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.ToApplicationResult();
    }
    
    // --- Role & Policy Management ---
    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<Result> AddToRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Result.Failure(["User not found."]);

        var result = await _userManager.AddToRoleAsync(user, role);
        return result.ToApplicationResult();
    }

    public async Task<Result> RemoveFromRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Result.Failure(["User not found."]);

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        return result.ToApplicationResult();
    }
    
    public async Task<IList<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return new List<string>();
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);
        var authResult = await _authorizationService.AuthorizeAsync(principal, policyName);
        return authResult.Succeeded;
    }

    public async Task<SignInResult> TwoFactorAuthenticatorSignInAsync(string twoFactorCode, bool isPersistent, bool rememberClient)
    {
        return await _signInManager.TwoFactorAuthenticatorSignInAsync(twoFactorCode, isPersistent, rememberClient);
    }

    public async Task<IUser?> GetTwoFactorAuthenticationUserAsync()
    {
        return await _signInManager.GetTwoFactorAuthenticationUserAsync();
    }
}

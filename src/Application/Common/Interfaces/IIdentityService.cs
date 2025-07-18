﻿using Microsoft.AspNetCore.Identity;
using SDI_Api.Application.Common.Models;
using SDI_Api.Domain.Entities;
using SDI_Api.Web.DTOs;

namespace SDI_Api.Application.Common.Interfaces;

public interface IIdentityService
{
    // --- ApplicationUser Management ---
    Task<string?> GetUserNameAsync(string userId);
    Task<IUser?> FindUserByIdAsync(string userId);
    Task<IEnumerable<IUser>> FindUsersByIdListAsync(List<string> ids);
    Task<IUser?> FindUserByEmailAsync(string email);
    Task<IUser?> FindUserByUsernameAsync(string username);
    Task<bool> GetTwoFactorEnabledAsync(IUser user);
    Task<bool> CheckPasswordAsync(IUser user, string password);
    Task<(Result Result, string UserId)> CreateUserAsync(string email, string password, string? firstName, string? lastName, CancellationToken cancellationToken); // Modified to include names for Member creation
    Task<Result> DeleteUserAsync(string userId);
    Task SignOutAsync();
    
    // --- Email & Phone ---
    Task<Result> SetEmailAsync(string userId, string newEmail);
    Task<string> GenerateEmailConfirmationTokenAsync(IUser user);
    Task<Result> ConfirmEmailAsync(string userId);
    Task<Result> SetPhoneNumberAsync(string userId, string? phoneNumber);
    
    // --- Password Management ---
    Task<Result> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
    Task<SignInResult> CheckPasswordSignInAsync(IUser? user, string password, bool lockoutOnFailure);
    Task<SignInResult> PasswordSignInAsync(IUser user, string password, bool isPersistent, bool lockoutOnFailure);
    Task<Result> ResetPasswordAsync(string userId, string token, string newPassword);
    Task<string> GeneratePasswordResetTokenAsync(IUser user);
    // --- Role & Policy Management ---
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<Result> AddToRoleAsync(string userId, string role);
    Task<Result> RemoveFromRoleAsync(string userId, string role);
    Task<IList<string>> GetUserRolesAsync(string userId);
    Task<bool> AuthorizeAsync(string userId, string policyName);
    
    // --- SignIn & Authentication ---
    Task<SignInResult> TwoFactorAuthenticatorSignInAsync(string userId, string twoFactorCode);
    Task<IUser?> GetTwoFactorAuthenticationUserAsync();
    Task<Result> EnableTwoFactorAuthenticationAsync(string userId);
    // TODO: refactor to return string instead of tuple
    Task<(string sharedKey, string authenticatorUri)> GenerateTwoFactorAuthenticatorKeyAsync(IUser user);
}

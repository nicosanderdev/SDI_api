namespace SDI_Api.Application.Common.Interfaces;

public interface ICompanyAccessService
{
    /// <summary>
    /// Validates if a user has access to a specific company or company filter mode.
    /// </summary>
    /// <param name="userId">The user ID to check access for</param>
    /// <param name="companyId">The company filter value (null, "all", "all-companies", or company UUID)</param>
    /// <returns>True if access is granted, throws exception otherwise</returns>
    /// <exception cref="ForbiddenAccessException">Thrown when user doesn't have access to the company</exception>
    /// <exception cref="CompanyNotFoundException">Thrown when the specified company doesn't exist</exception>
    /// <exception cref="InvalidCompanyFilterException">Thrown when the company filter value is invalid</exception>
    Task ValidateCompanyAccessAsync(Guid userId, string? companyId);

    /// <summary>
    /// Checks if a user has admin privileges for viewing all companies data.
    /// </summary>
    /// <param name="userId">The user ID to check</param>
    /// <returns>True if user has admin privileges</returns>
    Task<bool> HasAdminPrivilegesAsync(Guid userId);

    /// <summary>
    /// Gets all company IDs that a user has access to (including their own company memberships).
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>List of accessible company IDs</returns>
    Task<List<Guid>> GetAccessibleCompanyIdsAsync(Guid userId);
}

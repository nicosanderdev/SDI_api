using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Util.Extensions;

public static class CompanyFilteringExtensions
{
    /// <summary>
    /// Applies company filtering to EstateProperty queries based on the companyId parameter.
    /// </summary>
    /// <param name="query">The base query to filter</param>
    /// <param name="companyId">The company filter value</param>
    /// <param name="userId">The user ID for personal/company filtering</param>
    /// <param name="accessibleCompanyIds">List of company IDs the user has access to</param>
    /// <returns>Filtered query</returns>
    public static IQueryable<EstateProperty> ApplyCompanyFilter(
        this IQueryable<EstateProperty> query,
        string? companyId,
        Guid userId,
        List<Guid> accessibleCompanyIds)
    {
        if (string.IsNullOrEmpty(companyId))
            return query.Where(ep => ep.Owner.UserId == userId);

        switch (companyId.ToLower())
        {
            case "all":
                return query;

            case "all-companies":
                return query.Where(ep =>
                    ep.Owner.UserId == userId ||
                    accessibleCompanyIds.Contains(ep.Owner.Id));

            default:
                if (Guid.TryParse(companyId, out var companyGuid))
                {
                    return query.Where(ep =>
                        ep.Owner.Id != Guid.Empty && ep.Owner.Id == companyGuid);
                }
                return query.Where(ep => ep.Owner.UserId == userId);
        }
    }

    /// <summary>
    /// Applies company filtering to PropertyVisitLog queries based on the companyId parameter.
    /// </summary>
    /// <param name="query">The base query to filter</param>
    /// <param name="companyId">The company filter value</param>
    /// <param name="userId">The user ID for personal/company filtering</param>
    /// <param name="accessibleCompanyIds">List of company IDs the user has access to</param>
    /// <returns>Filtered query</returns>
    public static IQueryable<PropertyVisitLog> ApplyCompanyFilter(
        this IQueryable<PropertyVisitLog> query,
        string? companyId,
        Guid userId,
        List<Guid> accessibleCompanyIds)
    {
        if (string.IsNullOrEmpty(companyId))
            return query.Where(pvl => pvl.Property!.Owner.UserId == userId);

        switch (companyId.ToLower())
        {
            case "all":
                // All visit logs from all companies - no additional filtering needed
                // (access control is handled at the service level)
                return query;

            case "all-companies":
                // Visit logs from all companies the user belongs to
                return query.Where(pvl =>
                    pvl.Property!.Owner.UserId == userId || // Personal properties
                    accessibleCompanyIds.Contains(pvl.Property!.Owner.Id)); // Company properties

            default:
                // Specific company UUID
                if (Guid.TryParse(companyId, out var companyGuid))
                {
                    return query.Where(pvl =>
                        pvl.Property!.Owner.UserId == userId || // Personal properties (if user has access)
                        (pvl.Property!.Owner.Id != Guid.Empty && pvl.Property!.Owner.Id == companyGuid)); // Company properties
                }
                // Invalid company ID - should be caught by validation, but return original query as fallback
                return query.Where(pvl => pvl.Property!.Owner.UserId == userId);
        }
    }

    /// <summary>
    /// Applies company filtering to PropertyMessageLog queries based on the companyId parameter.
    /// </summary>
    /// <param name="query">The base query to filter</param>
    /// <param name="companyId">The company filter value</param>
    /// <param name="userId">The user ID for personal/company filtering</param>
    /// <param name="accessibleCompanyIds">List of company IDs the user has access to</param>
    /// <returns>Filtered query</returns>
    public static IQueryable<PropertyMessageLog> ApplyCompanyFilter(
        this IQueryable<PropertyMessageLog> query,
        string? companyId,
        Guid userId,
        List<Guid> accessibleCompanyIds)
    {
        if (string.IsNullOrEmpty(companyId))
        {
            // Personal properties only - filter by property ownership
            return query.Where(pml => pml.Property!.Owner.UserId == userId);
        }

        switch (companyId.ToLower())
        {
            case "all":
                // All message logs from all companies - no additional filtering needed
                // (access control is handled at the service level)
                return query;

            case "all-companies":
                // Message logs from all companies the user belongs to
                return query.Where(pml =>
                    pml.Property!.Owner.UserId == userId || // Personal properties
                    accessibleCompanyIds.Contains(pml.Property!.Owner.Id)); // Company properties

            default:
                // Specific company UUID
                if (Guid.TryParse(companyId, out var companyGuid))
                {
                    return query.Where(pml =>
                        pml.Property!.Owner.UserId == userId || // Personal properties (if user has access)
                        (pml.Property!.Owner.Id != Guid.Empty && pml.Property!.Owner.Id == companyGuid)); // Company properties
                }
                // Invalid company ID - should be caught by validation, but return original query as fallback
                return query.Where(pml => pml.Property!.Owner.UserId == userId);
        }
    }
}

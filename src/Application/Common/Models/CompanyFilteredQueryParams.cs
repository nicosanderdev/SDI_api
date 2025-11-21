namespace SDI_Api.Application.Common.Models;

/// <summary>
/// Base class for query parameters that support company filtering
/// </summary>
public class CompanyFilteredQueryParams
{
    /// <summary>
    /// Company filter parameter.
    /// - null or empty: Show user's personal properties only
    /// - "all": Show all properties from all companies (admin view)
    /// - "all-companies": Show properties from all companies the user belongs to
    /// - "{company-uuid}": Show properties from specific company only
    /// </summary>
    public string? CompanyId { get; set; }

    /// <summary>
    /// Period parameter for report endpoints (last7days | last30days | last90days | thisyear)
    /// </summary>
    public string? Period { get; set; }
}

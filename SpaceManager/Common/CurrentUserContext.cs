namespace SpaceManager.Common;

/// <summary>
/// Encapsulates the authenticated caller's identity, role, and company affiliation.
/// Passed from controllers to services to enforce tenant isolation and role-based data scoping.
/// </summary>
/// <param name="UserId">The unique identifier of the user (from ClaimTypes.NameIdentifier).</param>
/// <param name="Role">The primary role of the user (e.g. "Admin", "CompanyAdmin", "User").</param>
/// <param name="CompanyId">The company ID if the user is affiliated with a company; otherwise null.</param>
public record CurrentUserContext(
    string UserId,
    string Role,
    int? CompanyId = null
)
{
    public bool IsAdmin => Role == "Admin";
    public bool IsCompanyAdmin => Role == "CompanyAdmin";
    public bool IsRegularUser => !IsAdmin && !IsCompanyAdmin;

    /// <summary>
    /// Checks whether the caller is authorized to manage a specific company's resources.
    /// System Admins can manage any company; Company Admins can only manage their own company.
    /// </summary>
    public bool CanManageCompany(int targetCompanyId) =>
        IsAdmin || (IsCompanyAdmin && CompanyId == targetCompanyId);
}
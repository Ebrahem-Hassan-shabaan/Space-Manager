using System.Security.Claims;

namespace SpaceManager.Common;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Extracts the strongly-typed caller context from the current HTTP user's claims.
    /// Prioritizes privileged roles (Admin > CompanyAdmin > User).
    /// </summary>
    public static CurrentUserContext GetUserContext(this ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        string primaryRole = "User";
        if (roles.Contains("Admin"))
        {
            primaryRole = "Admin";
        }
        else if (roles.Contains("CompanyAdmin"))
        {
            primaryRole = "CompanyAdmin";
        }
        else if (roles.Any())
        {
            primaryRole = roles.First();
        }

        var companyClaim = user.FindFirstValue("CompanyId");
        int? companyId = int.TryParse(companyClaim, out var id) ? id : null;

        return new CurrentUserContext(userId, primaryRole, companyId);
    }
}

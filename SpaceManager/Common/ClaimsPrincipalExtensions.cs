using System.Security.Claims;

namespace SpaceManager.Common;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Extracts the strongly-typed caller context from the current HTTP user's claims.
    /// </summary>
    public static CurrentUserContext GetUserContext(this ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = user.FindFirstValue(ClaimTypes.Role) ?? "User";
        var companyClaim = user.FindFirstValue("CompanyId");
        int? companyId = int.TryParse(companyClaim, out var id) ? id : null;

        return new CurrentUserContext(userId, role, companyId);
    }
}
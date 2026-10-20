using SpaceManager.DTOs.Companies;
using SpaceManager.Models;

namespace SpaceManager.Mappings;

/// <summary>
/// Provides pure static projection and mapping extensions for Company domain entities and DTOs.
/// </summary>
public static class CompanyMappings
{
    /// <summary>
    /// Projects a <see cref="Company"/> entity into a <see cref="CompanyResponseDto"/>.
    /// </summary>
    public static CompanyResponseDto ToResponseDto(this Company company) =>
        new(
            company.Id,
            company.Name,
            company.TaxRegistrationNumber,
            company.BillingAddress,
            company.ContactEmail,
            company.CreatedAt,
            company.Users?.Count ?? 0
        );

    /// <summary>
    /// Projects an <see cref="ApplicationUser"/> member into a <see cref="CompanyMemberResponseDto"/>.
    /// </summary>
    public static CompanyMemberResponseDto ToMemberDto(this ApplicationUser user, bool isCompanyAdmin = false) =>
        new(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            isCompanyAdmin,
            user.RegisteredAt
        );
}

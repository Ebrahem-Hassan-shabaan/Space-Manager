using SpaceManager.Common;
using SpaceManager.DTOs.Companies;

namespace SpaceManager.Services.Interfaces;

/// <summary>
/// Defines business service operations for managing corporate companies and member assignments.
/// </summary>
public interface ICompanyService
{
    /// <summary>
    /// Retrieves all companies in the system. Restricted to administrators.
    /// </summary>
    /// <returns>A collection of company response DTOs.</returns>
    Task<IEnumerable<CompanyResponseDto>> GetAllAsync();

    /// <summary>
    /// Retrieves a company by its unique identifier. Restricted to administrators or members of the company.
    /// </summary>
    /// <param name="id">The unique identifier of the company.</param>
    /// <param name="caller">The identity context of the caller.</param>
    /// <returns>A service result containing the company response DTO if found and authorized.</returns>
    Task<ServiceResult<CompanyResponseDto>> GetByIdAsync(int id, CurrentUserContext caller);

    /// <summary>
    /// Creates a new corporate company and optionally designates an initial Company Admin.
    /// </summary>
    /// <param name="dto">The company creation payload.</param>
    /// <returns>A service result containing the created company response DTO.</returns>
    Task<ServiceResult<CompanyResponseDto>> CreateAsync(CreateCompanyDto dto);

    /// <summary>
    /// Updates an existing company profile.
    /// </summary>
    /// <param name="id">The unique identifier of the company to update.</param>
    /// <param name="dto">The update payload.</param>
    /// <returns>A service result containing the updated company response DTO.</returns>
    Task<ServiceResult<CompanyResponseDto>> UpdateAsync(int id, UpdateCompanyDto dto);

    /// <summary>
    /// Deletes a company by its unique identifier. (Admin only)
    /// Associated member accounts have their CompanyId set to null.
    /// </summary>
    /// <param name="id">The unique identifier of the company to delete.</param>
    /// <returns>True if the company was deleted; false if not found.</returns>
    Task<ServiceResult> DeleteAsync(int id);

    /// <summary>
    /// Retrieves all active members/employees belonging to a specific company.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company.</param>
    /// <param name="caller">The identity context of the caller.</param>
    /// <returns>A service result containing the list of company members.</returns>
    Task<ServiceResult<IEnumerable<CompanyMemberResponseDto>>> GetMembersAsync(int companyId, CurrentUserContext caller);

    /// <summary>
    /// Attaches an existing registered user to a company by email.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company.</param>
    /// <param name="email">The registered email of the user to add.</param>
    /// <param name="caller">The identity context of the caller.</param>
    /// <returns>A service result indicating success or the specific validation error.</returns>
    Task<ServiceResult> AddMemberAsync(int companyId, string email, CurrentUserContext caller);

    /// <summary>
    /// Removes an employee from a company, unlinking their account.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company.</param>
    /// <param name="userId">The unique identifier of the member to remove.</param>
    /// <param name="caller">The identity context of the caller.</param>
    /// <returns>A service result indicating success or failure.</returns>
    Task<ServiceResult> RemoveMemberAsync(int companyId, string userId, CurrentUserContext caller);
    Task<ServiceResult> SetCompanyAdminAsync(int companyId, string email);
}

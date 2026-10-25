namespace SpaceManager.DTOs.Companies;

/// <summary>
/// Data transfer object representing a company employee/member.
/// </summary>
/// <param name="UserId">The unique identifier of the user account.</param>
/// <param name="FullName">The full name of the employee.</param>
/// <param name="Email">The email address of the employee.</param>
/// <param name="RegisteredAt">The timestamp when the user registered.</param>
public record CompanyMemberResponseDto(
    string UserId,
    string FullName,
    string Email,
    DateTime RegisteredAt
);

using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.Companies;

/// <summary>
/// Payload used to grant company administrator privileges to an existing company member.
/// </summary>
/// <param name="Email">The registered email address of the employee to elevate as company admin.</param>
public record AssignCompanyAdminDto(
    [Required, EmailAddress] string Email
);

using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.Companies;

/// <summary>
/// Data transfer object used to add an existing registered user to a company.
/// </summary>
/// <param name="Email">The registered email address of the user to attach to the company.</param>
public record AddCompanyMemberDto(
    [Required]
    [EmailAddress]
    string Email
);

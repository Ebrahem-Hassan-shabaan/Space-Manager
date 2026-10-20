using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.Companies;

/// <summary>
/// Data transfer object used to update an existing company profile.
/// </summary>
/// <param name="Name">The registered legal business name.</param>
/// <param name="TaxRegistrationNumber">The official tax identification or commercial registry number.</param>
/// <param name="BillingAddress">The primary corporate billing street address.</param>
/// <param name="ContactEmail">The official billing and primary contact email.</param>
public record UpdateCompanyDto(
    [Required]
    [MaxLength(100)]
    string Name,

    [Required]
    [MaxLength(50)]
    string TaxRegistrationNumber,

    [Required]
    [MaxLength(200)]
    string BillingAddress,

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    string ContactEmail
);

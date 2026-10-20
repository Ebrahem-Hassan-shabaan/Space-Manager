using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.Companies;

/// <summary>
/// Data transfer object used to register a new corporate company.
/// </summary>
/// <param name="Name">The registered legal business name.</param>
/// <param name="TaxRegistrationNumber">The official tax identification or commercial registry number.</param>
/// <param name="BillingAddress">The primary corporate billing street address.</param>
/// <param name="ContactEmail">The official billing and primary contact email.</param>
/// <param name="AdminEmail">Optional email of an existing user to designate as the primary Company Admin.</param>
public record CreateCompanyDto(
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
    string ContactEmail,

    [EmailAddress]
    string? AdminEmail = null
);

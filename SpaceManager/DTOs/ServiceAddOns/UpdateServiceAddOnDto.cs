using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.ServiceAddOns;

/// <summary>
/// Data transfer object used to update an existing service add-on.
/// </summary>
/// <param name="Name">The updated name of the service.</param>
/// <param name="Price">The updated price of the service.</param>
/// <param name="IsAvailable">The updated availability status.</param>
public record UpdateServiceAddOnDto(
    [Required]
    [MaxLength(100)]
    string Name,

    [Required]
    [Range(0.01, 100000)]
    decimal Price,

    bool IsAvailable
);

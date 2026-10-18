using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.ServiceAddOns;

/// <summary>
/// Data transfer object used to create a new service add-on.
/// </summary>
/// <param name="Name">The name of the service add-on.</param>
/// <param name="Price">The price of the service.</param>
/// <param name="IsAvailable">Whether the service is immediately available for selection.</param>
public record CreateServiceAddOnDto(
    [Required]
    [MaxLength(100)]
    string Name,

    [Required]
    [Range(0.01, 100000)]
    decimal Price,

    bool IsAvailable = true
);

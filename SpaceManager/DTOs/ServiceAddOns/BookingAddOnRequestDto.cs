using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.ServiceAddOns;

/// <summary>
/// Data transfer object for requesting an add-on during booking creation.
/// </summary>
/// <param name="ServiceAddOnId">The unique identifier of the service add-on.</param>
/// <param name="Quantity">The quantity requested (default is 1).</param>
public record BookingAddOnRequestDto(
    [Required]
    int ServiceAddOnId,

    [Required]
    [Range(1, 100)]
    int Quantity = 1
);

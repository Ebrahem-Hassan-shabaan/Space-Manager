using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.Amenities;

/// <summary>
/// Data transfer object used to update an existing amenity.
/// </summary>
/// <param name="Name">The updated name of the amenity.</param>
/// <param name="Description">The updated description of the amenity.</param>
public record UpdateAmenityDto(
    [Required]
    [MaxLength(100)]
    string Name,

    [MaxLength(600)]
    string? Description
);

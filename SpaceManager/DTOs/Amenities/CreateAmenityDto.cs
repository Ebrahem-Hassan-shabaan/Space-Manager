using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.Amenities;

/// <summary>
/// Data transfer object used to create a new amenity.
/// </summary>
/// <param name="Name">The name of the amenity.</param>
/// <param name="Description">An optional description of the amenity.</param>
public record CreateAmenityDto(
    [Required]
    [MaxLength(100)]
    string Name,

    [MaxLength(600)]
    string? Description
);

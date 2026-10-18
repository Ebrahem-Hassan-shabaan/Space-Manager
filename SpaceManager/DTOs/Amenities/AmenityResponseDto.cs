namespace SpaceManager.DTOs.Amenities;

/// <summary>
/// Data transfer object representing an amenity.
/// </summary>
/// <param name="Id">The unique identifier of the amenity.</param>
/// <param name="Name">The name of the amenity.</param>
/// <param name="Description">A brief description of the amenity.</param>
public record AmenityResponseDto(
    int Id,
    string Name,
    string? Description
);

namespace SpaceManager.DTOs.Amenities;

/// <summary>
/// Data transfer object representing an amenity associated with a workspace.
/// </summary>
/// <param name="AmenityId">The unique identifier of the amenity.</param>
/// <param name="Name">The name of the amenity.</param>
/// <param name="Description">The description of the amenity.</param>
/// <param name="IsOperational">Indicates if the amenity is currently operational in this workspace.</param>
public record WorkspaceAmenityResponseDto(
    int AmenityId,
    string Name,
    string? Description,
    bool IsOperational
);

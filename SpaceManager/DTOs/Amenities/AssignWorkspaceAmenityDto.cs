using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.Amenities;

/// <summary>
/// Data transfer object used to assign or update an amenity on a workspace.
/// </summary>
/// <param name="AmenityId">The unique identifier of the amenity to assign.</param>
/// <param name="IsOperational">Whether the amenity is currently operational in this workspace.</param>
public record AssignWorkspaceAmenityDto(
    [Required]
    int AmenityId,

    bool IsOperational = true
);

using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.Workspaces;

/// <summary>
/// Data transfer object used to create a new workspace unit.
/// </summary>
/// <param name="Name">The display name or identifier of the workspace.</param>
/// <param name="PricePerHour">The hourly rental rate.</param>
/// <param name="Capacity">Maximum occupant or seating capacity.</param>
/// <param name="WorkspaceTypeId">Foreign key of the workspace category.</param>
public record CreateWorkspaceDto(
    [Required, MaxLength(100)] string Name,
    [Range(0.01, 100000)] decimal PricePerHour,
    [Range(1, 1000)] int Capacity,
    [Required] int WorkspaceTypeId
);

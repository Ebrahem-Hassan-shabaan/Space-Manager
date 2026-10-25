namespace SpaceManager.DTOs.Workspaces;

/// <summary>
/// Response projection representing workspace details and availability.
/// </summary>
/// <param name="Id">The unique identifier of the workspace.</param>
/// <param name="Name">The display name or room code of the workspace.</param>
/// <param name="PricePerHour">The hourly rental rate.</param>
/// <param name="Capacity">Maximum occupant or seating capacity.</param>
/// <param name="IsAvailable">Operational availability flag.</param>
/// <param name="WorkspaceTypeId">Foreign key of the workspace category.</param>
/// <param name="WorkspaceTypeName">Display name of the category classification.</param>
public record WorkspaceResponseDto(
    int Id,
    string Name,
    decimal PricePerHour,
    int Capacity,
    bool IsAvailable,
    int WorkspaceTypeId,
    string WorkspaceTypeName
);

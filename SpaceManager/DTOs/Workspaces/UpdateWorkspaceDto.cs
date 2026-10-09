namespace SpaceManager.DTOs.Workspaces;

public record UpdateWorkspaceDto(
    string Name,
    decimal PricePerHour,
    int Capacity,
    int WorkspaceTypeId,
    bool IsAvailable
);

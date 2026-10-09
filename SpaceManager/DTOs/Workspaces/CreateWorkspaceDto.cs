namespace SpaceManager.DTOs.Workspaces;

public record CreateWorkspaceDto(
    string Name,
    decimal PricePerHour,
    int Capacity,
    int WorkspaceTypeId
);

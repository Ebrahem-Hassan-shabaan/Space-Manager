namespace SpaceManager.DTOs.Workspaces;

public record WorkspaceResponseDto(
    int Id,
    string Name,
    decimal PricePerHour,
    int Capacity,
    bool IsAvailable,
    int WorkspaceTypeId,
    string WorkspaceTypeName
);

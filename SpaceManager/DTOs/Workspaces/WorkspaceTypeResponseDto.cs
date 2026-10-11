namespace SpaceManager.DTOs.Workspaces;

/// <summary>
/// Data transfer object representing a workspace type.
/// </summary>
/// <param name="Id">The unique identifier of the workspace type.</param>
/// <param name="Name">The display name of the workspace type (e.g., Hot Desk, Meeting Room).</param>
public record WorkspaceTypeResponseDto(
    int Id,
    string Name
);

namespace SpaceManager.Models;

/// <summary>
/// Represents a classification or category of workspace (e.g., Hot Desk, Dedicated Desk, Conference Room, Private Office).
/// </summary>
public class WorkspaceType
{
    /// <summary>
    /// Gets or sets the unique identifier of the workspace type.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name or category label of the workspace type.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of workspaces belonging to this category.
    /// </summary>
    public ICollection<Workspace> Workspaces { get; set; } = new List<Workspace>();
}

namespace SpaceManager.Models;

/// <summary>
/// Represents a physical or digital amenity available within workspaces (e.g., High-Speed Wi-Fi, Standing Desk, Monitor).
/// </summary>
public class Amenity
{
    /// <summary>
    /// Gets or sets the unique identifier of the amenity.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the amenity.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional description or specification of the amenity.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the collection of workspace assignments linked to this amenity.
    /// </summary>
    public ICollection<WorkspaceAmenity> WorkspaceAmenities { get; set; } = new List<WorkspaceAmenity>();
}

namespace SpaceManager.Models;

/// <summary>
/// Represents a join entity mapping an amenity to a specific workspace.
/// </summary>
public class WorkspaceAmenity
{
    /// <summary>
    /// Gets or sets the foreign key of the associated workspace.
    /// </summary>
    public int WorkspaceId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the associated workspace.
    /// </summary>
    public Workspace Workspace { get; set; } = null!;

    /// <summary>
    /// Gets or sets the foreign key of the associated amenity.
    /// </summary>
    public int AmenityId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the associated amenity.
    /// </summary>
    public Amenity Amenity { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether this amenity is currently functioning and operational in the space.
    /// </summary>
    public bool IsOperational { get; set; } = true;
}

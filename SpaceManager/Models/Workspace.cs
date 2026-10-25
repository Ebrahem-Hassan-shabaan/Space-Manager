namespace SpaceManager.Models;

/// <summary>
/// Represents a physical bookable area or office unit (e.g., Hot Desk, Meeting Room, Private Office).
/// </summary>
public class Workspace
{
    /// <summary>
    /// Gets or sets the unique identifier of the workspace.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the display name or room code of the workspace.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base rental fee charged per hour.
    /// </summary>
    public decimal PricePerHour { get; set; }

    /// <summary>
    /// Gets or sets the maximum seating or occupant capacity of the space.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this workspace is currently operational and available for booking.
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Gets or sets the foreign key of the classification category for this workspace.
    /// </summary>
    public int WorkspaceTypeId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the workspace type category.
    /// </summary>
    public WorkspaceType WorkspaceType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of reservations booked for this workspace.
    /// </summary>
    public ICollection<WorkspaceBooking> WorkspaceBookings { get; set; } = new List<WorkspaceBooking>();

    /// <summary>
    /// Gets or sets the collection of maintenance issues filed for this workspace.
    /// </summary>
    public ICollection<MaintenanceTicket> MaintenanceTickets { get; set; } = new List<MaintenanceTicket>();

    /// <summary>
    /// Gets or sets the collection of amenity assignments enabled for this workspace.
    /// </summary>
    public ICollection<WorkspaceAmenity> WorkspaceAmenities { get; set; } = new List<WorkspaceAmenity>();
}

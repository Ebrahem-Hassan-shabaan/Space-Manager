using SpaceManager.Enums;

namespace SpaceManager.Models;

/// <summary>
/// Represents a calendar reservation of a workspace by an individual or corporate user.
/// </summary>
public class WorkspaceBooking
{
    /// <summary>
    /// Gets or sets the unique identifier of the workspace reservation.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the scheduled start date and time of the reservation.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the scheduled end date and time of the reservation.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Gets or sets the calculated grand total cost (space hourly duration + add-ons).
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Gets or sets the current lifecycle status of the reservation (e.g., Confirmed, Completed, Cancelled).
    /// </summary>
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    /// <summary>
    /// Gets or sets the foreign key of the user who placed the booking.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the navigation property to the user who placed the booking.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the foreign key of the reserved workspace.
    /// </summary>
    public int WorkspaceId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the reserved workspace.
    /// </summary>
    public Workspace Workspace { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of visitor passes created for attendees of this reservation.
    /// </summary>
    public ICollection<VisitorPass> VisitorPasses { get; set; } = new List<VisitorPass>();

    /// <summary>
    /// Gets or sets the collection of add-on products or services appended to this reservation.
    /// </summary>
    public ICollection<BookingAddOn> BookingAddOns { get; set; } = new List<BookingAddOn>();

    /// <summary>
    /// Gets or sets the optional invoice generated to bill for this reservation.
    /// </summary>
    public Invoice? Invoice { get; set; }
}

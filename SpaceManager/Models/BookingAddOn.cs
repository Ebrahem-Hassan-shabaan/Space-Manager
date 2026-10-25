namespace SpaceManager.Models;

/// <summary>
/// Represents a join entity between a workspace booking and a requested service add-on (e.g., catering, projector, espresso).
/// </summary>
public class BookingAddOn
{
    /// <summary>
    /// Gets or sets the foreign key of the associated workspace booking.
    /// </summary>
    public int WorkspaceBookingId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the associated workspace booking.
    /// </summary>
    public WorkspaceBooking WorkspaceBooking { get; set; } = null!;

    /// <summary>
    /// Gets or sets the foreign key of the requested service add-on.
    /// </summary>
    public int ServiceAddOnId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the requested service add-on.
    /// </summary>
    public ServiceAddOn ServiceAddOn { get; set; } = null!;

    /// <summary>
    /// Gets or sets the quantity of the add-on requested for the reservation.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the calculated total price for this add-on item (Quantity * UnitPrice).
    /// </summary>
    public decimal TotalPrice { get; set; }
}

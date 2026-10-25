namespace SpaceManager.Models;

/// <summary>
/// Represents a security/reception guest pass issued for visitors attending a workspace reservation.
/// </summary>
public class VisitorPass
{
    /// <summary>
    /// Gets or sets the unique identifier of the visitor pass.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the full legal or display name of the visiting guest.
    /// </summary>
    public string GuestName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address of the visiting guest for invitations and pass delivery.
    /// </summary>
    public string GuestEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the anticipated date and time of arrival for the visitor.
    /// </summary>
    public DateTime ExpectedArrival { get; set; }

    /// <summary>
    /// Gets or sets the actual timestamp when the visitor checked in at reception, or null if pending.
    /// </summary>
    public DateTime? ActualCheckInTime { get; set; }

    /// <summary>
    /// Gets or sets the foreign key of the host member who invited the visitor.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the navigation property to the host member.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the foreign key of the workspace booking associated with this guest visit.
    /// </summary>
    public int WorkspaceBookingId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the associated workspace booking.
    /// </summary>
    public WorkspaceBooking WorkspaceBooking { get; set; } = null!;
}

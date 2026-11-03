namespace SpaceManager.Models;

/// <summary>
/// Represents an in-app alert or system message dispatched to a platform user.
/// </summary>
public class Notification
{
    /// <summary>
    /// Gets or sets the primary key identifier of the notification.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the brief headline or title of the notification (e.g., 'Booking Confirmed').
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed explanatory body of the notification.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional relative URL link navigating to the target entity (e.g., '/bookings', '/invoices').
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the recipient has acknowledged/read the notification.
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Gets the timestamp indicating when the notification was generated.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the foreign key of the user receiving the notification.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the navigation property to the recipient user.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Gets or sets an optional foreign key link to a related booking.
    /// </summary>
    public int? WorkspaceBookingId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the related booking, if applicable.
    /// </summary>
    public WorkspaceBooking? WorkspaceBooking { get; set; }

    /// <summary>
    /// Gets or sets an optional foreign key link to a related corporate account.
    /// </summary>
    public int? CompanyId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the related company, if applicable.
    /// </summary>
    public Company? Company { get; set; }
}

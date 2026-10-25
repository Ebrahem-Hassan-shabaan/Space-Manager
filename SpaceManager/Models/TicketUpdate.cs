namespace SpaceManager.Models;

/// <summary>
/// Represents a message, progress note, or staff response attached to a maintenance ticket.
/// </summary>
public class TicketUpdate
{
    /// <summary>
    /// Gets or sets the unique identifier of the ticket update.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the narrative message content or reply text.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets the timestamp when this update message was posted.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether this message was authored by an administrator or facility staff member.
    /// </summary>
    public bool IsAdminReply { get; set; } = false;

    /// <summary>
    /// Gets or sets the foreign key of the parent maintenance ticket.
    /// </summary>
    public int MaintenanceTicketId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the parent maintenance ticket.
    /// </summary>
    public MaintenanceTicket MaintenanceTicket { get; set; } = null!;

    /// <summary>
    /// Gets or sets the foreign key of the user who authored this message.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the navigation property to the user who authored this message.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
}

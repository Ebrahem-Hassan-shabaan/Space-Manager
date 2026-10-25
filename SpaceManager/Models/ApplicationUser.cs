using Microsoft.AspNetCore.Identity;

namespace SpaceManager.Models;

/// <summary>
/// Represents an application user within the SpaceManager platform, extending ASP.NET Core Identity.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Gets or sets the full legal or display name of the user.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the timestamp indicating when the user registered on the platform.
    /// </summary>
    public DateTime RegisteredAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the collection of workspace bookings initiated by this user.
    /// </summary>
    public ICollection<WorkspaceBooking> WorkspaceBookings { get; set; } = new List<WorkspaceBooking>();

    /// <summary>
    /// Gets or sets the collection of visitor passes created by this user.
    /// </summary>
    public ICollection<VisitorPass> VisitorPasses { get; set; } = new List<VisitorPass>();

    /// <summary>
    /// Gets or sets the collection of ticket updates or messages authored by this user.
    /// </summary>
    public ICollection<TicketUpdate> TicketUpdates { get; set; } = new List<TicketUpdate>();

    /// <summary>
    /// Gets or sets the collection of maintenance tickets submitted by this user.
    /// </summary>
    public ICollection<MaintenanceTicket> MaintenanceTickets { get; set; } = new List<MaintenanceTicket>();

    /// <summary>
    /// Gets or sets the collection of invoices billed to this user.
    /// </summary>
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    /// <summary>
    /// Gets or sets the corporate company this user is affiliated with, if applicable.
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Gets or sets the optional foreign key for the corporate company affiliation.
    /// </summary>
    public int? CompanyId { get; set; }
}

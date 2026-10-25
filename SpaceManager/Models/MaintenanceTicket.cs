using SpaceManager.Enums;

namespace SpaceManager.Models;

/// <summary>
/// Represents a facility maintenance request or operational issue submitted for a workspace.
/// </summary>
public class MaintenanceTicket
{
    /// <summary>
    /// Gets or sets the unique identifier of the maintenance ticket.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the brief headline or title describing the maintenance issue.
    /// </summary>
    public string IssueTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full narrative description of the issue or malfunction.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the severity or emergency level of the ticket (e.g., Low, Medium, High, Critical).
    /// </summary>
    public TicketEmergencyLevel EmergencyLevel { get; set; } = TicketEmergencyLevel.Medium;

    /// <summary>
    /// Gets or sets the resolution status of the ticket (e.g., Open, InProgress, Resolved, Closed).
    /// </summary>
    public TicketStatus Status { get; set; } = TicketStatus.Open;

    /// <summary>
    /// Gets the timestamp when the issue was reported.
    /// </summary>
    public DateTime ReportedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the foreign key of the affected workspace.
    /// </summary>
    public int WorkspaceId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the affected workspace.
    /// </summary>
    public Workspace Workspace { get; set; } = null!;

    /// <summary>
    /// Gets or sets the foreign key of the user who submitted the ticket.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the navigation property to the user who submitted the ticket.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of updates, notes, and staff responses logged for this ticket.
    /// </summary>
    public ICollection<TicketUpdate> TicketUpdates { get; set; } = new List<TicketUpdate>();
}

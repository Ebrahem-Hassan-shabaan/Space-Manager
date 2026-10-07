using SpaceManager.Enums;

namespace SpaceManager.Models;

public class MaintenanceTicket
{
    public int Id { get; set; }
    public string IssueTitle { get; set; }
    public string Description { get; set; }
    public TicketEmergencyLevel EmergencyLevel { get; set; } = TicketEmergencyLevel.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public DateTime ReportedAt { get; init; } = DateTime.UtcNow;

    public int WorkspaceId { get; set; }
    public Workspace Workspace { get; set; }

    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public ICollection<TicketUpdate> TicketUpdates { get; set; } = new List<TicketUpdate>();
}

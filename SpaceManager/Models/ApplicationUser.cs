using Microsoft.AspNetCore.Identity;

namespace SpaceManager.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }
    public DateTime RegisteredAt { get; init; } = DateTime.UtcNow;

    public ICollection<WorkspaceBooking> WorkspaceBookings { get; set; } = new List<WorkspaceBooking>();
    public ICollection<VisitorPass> VisitorPasses { get; set; } = new List<VisitorPass>();
    public ICollection<TicketUpdate> TicketUpdates { get; set; } = new List<TicketUpdate>();
    public ICollection<MaintenanceTicket> MaintenanceTickets { get; set; } = new List<MaintenanceTicket>();
}

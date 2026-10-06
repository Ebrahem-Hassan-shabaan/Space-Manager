using Microsoft.AspNetCore.Identity;
namespace SpaceManager.Models

{
    public class ApplicationUser:IdentityUser
    {
        public string FullName { get; set; } 
        public DateTime RegisteredAt { get; init; } = DateTime.UtcNow;
        public ICollection<WorkspaceBooking> WorkspaceBookings { get; set; }
        public ICollection<VisitorPass> VisitorPasses { get; set; }

        public ICollection<TicketUpdate> TicketUpdates { get; set; }
        public ICollection<MaintenanceTicket> MaintenanceTickets { get; set; }







    }
}

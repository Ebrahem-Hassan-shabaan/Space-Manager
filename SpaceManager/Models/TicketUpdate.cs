namespace SpaceManager.Models
{
    public class TicketUpdate
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        public bool  IsAdminReply { get; set; } = false;

        public int MaintenanceTicketId { get; set; }
        public MaintenanceTicket MaintenanceTicket { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}

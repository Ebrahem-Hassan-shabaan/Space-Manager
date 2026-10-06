namespace SpaceManager.Models
{
    public class Workspace
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Decimal PricePerHour { get; set; }
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; }= true;
        public int WorkspaceTypeId { get; set; }
        public WorkspaceType WorkspaceType { get; set; } 
        
        public ICollection<WorkspaceBooking> WorkspaceBookings { get; set; }
        public ICollection<MaintenanceTicket> MaintenanceTickets { get; set; }

    }
}

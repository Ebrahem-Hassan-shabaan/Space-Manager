namespace SpaceManager.Models
{
    public class WorkspaceBooking
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public decimal TotalCost { get; set; }

        public Enums.BookingStatus Status { get; set; } = Enums.BookingStatus.Confirmed;
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; }

        public ICollection<VisitorPass> VisitorPasses { get; set; }
    }
}

using SpaceManager.Enums;

namespace SpaceManager.Models;

public class WorkspaceBooking
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalCost { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public int WorkspaceId { get; set; }
    public Workspace Workspace { get; set; }

    public ICollection<VisitorPass> VisitorPasses { get; set; } = new List<VisitorPass>();
}

namespace SpaceManager.Models;

public class VisitorPass
{
    public int Id { get; set; }
    public string GuestName { get; set; }
    public string GuestEmail { get; set; }
    public DateTime ExpectedArrival { get; set; }
    public DateTime? ActualCheckInTime { get; set; }

    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public int WorkspaceBookingId { get; set; }
    public WorkspaceBooking WorkspaceBooking { get; set; }
}

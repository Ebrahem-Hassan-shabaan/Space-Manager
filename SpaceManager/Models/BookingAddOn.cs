namespace SpaceManager.Models;

public class BookingAddOn
{
    public int WorkspaceBookingId { get; set; }
    public WorkspaceBooking WorkspaceBooking { get; set; }
    public int ServiceAddOnId { get; set; }
    public ServiceAddOn ServiceAddOn { get; set; }
    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

}

namespace SpaceManager.Models;

public class ServiceAddOn
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public ICollection<BookingAddOn> BookingAddOns { get; set; } = new List<BookingAddOn>();
}

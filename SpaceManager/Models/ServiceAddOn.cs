namespace SpaceManager.Models;

/// <summary>
/// Represents an optional product or service that can be appended to a workspace booking (e.g., Catering, HD Projector, Whiteboard kit).
/// </summary>
public class ServiceAddOn
{
    /// <summary>
    /// Gets or sets the unique identifier of the service add-on.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name or label of the service add-on.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unit price charged per booking or quantity.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this add-on is currently active and available for selection.
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Gets or sets the collection of booking items that include this service add-on.
    /// </summary>
    public ICollection<BookingAddOn> BookingAddOns { get; set; } = new List<BookingAddOn>();
}

namespace SpaceManager.Models;

/// <summary>
/// Represents a corporate client or business organization registered on the platform.
/// </summary>
public class Company
{
    /// <summary>
    /// Gets or sets the unique identifier of the company.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the legal business or registered corporate name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the corporate tax registration or commercial identification number.
    /// </summary>
    public string TaxRegistrationNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the official physical or billing address for corporate invoicing.
    /// </summary>
    public string BillingAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets the timestamp when the company account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the primary corporate contact or billing email address.
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of employees or members associated with this company.
    /// </summary>
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    /// <summary>
    /// Gets or sets the collection of billing invoices issued to this corporate account.
    /// </summary>
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

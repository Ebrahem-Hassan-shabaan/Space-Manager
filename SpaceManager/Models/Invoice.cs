using SpaceManager.Enums;

namespace SpaceManager.Models;

/// <summary>
/// Represents a billing invoice issued to an individual member or corporate company.
/// </summary>
public class Invoice
{
    /// <summary>
    /// Gets or sets the unique identifier of the invoice.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the total monetary amount billed on this invoice.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the invoice was issued.
    /// </summary>
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the payment due date for the invoice.
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Gets or sets the current lifecycle status of the invoice.
    /// </summary>
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Issued;

    /// <summary>
    /// Gets or sets the optional foreign key of the individual user billed.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the billed user.
    /// </summary>
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Gets or sets the optional foreign key of the corporate company billed.
    /// </summary>
    public int? CompanyId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the billed company.
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Gets or sets the optional foreign key of the associated workspace booking.
    /// </summary>
    public int? WorkspaceBookingId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the associated workspace booking.
    /// </summary>
    public WorkspaceBooking? WorkspaceBooking { get; set; }

    /// <summary>
    /// Gets or sets the collection of payments applied toward settling this invoice.
    /// </summary>
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

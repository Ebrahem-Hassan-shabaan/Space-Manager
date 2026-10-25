namespace SpaceManager.Enums;

/// <summary>
/// Represents the financial lifecycle state of an invoice.
/// </summary>
public enum InvoiceStatus
{
    Issued = 1,
    Paid = 2,
    PartiallyPaid = 3,
    Overdue = 4,
    Cancelled = 5,
    Refunded = 6
}

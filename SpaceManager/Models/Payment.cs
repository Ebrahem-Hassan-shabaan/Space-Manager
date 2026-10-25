using SpaceManager.Enums;

namespace SpaceManager.Models;

/// <summary>
/// Represents a payment transaction executed toward settling an invoice.
/// </summary>
public class Payment
{
    /// <summary>
    /// Gets or sets the unique identifier of the payment.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the monetary amount paid in this transaction.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the payment was completed.
    /// </summary>
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the payment channel or instrument used (e.g., CreditCard, DebitCard, BankTransfer, Cash).
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    /// <summary>
    /// Gets or sets the external transaction ID, gateway reference code, or manual receipt voucher.
    /// </summary>
    public string TransactionReference { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the foreign key of the invoice this payment was applied to.
    /// </summary>
    public int InvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the associated invoice.
    /// </summary>
    public Invoice Invoice { get; set; } = null!;
}

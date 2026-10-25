using System.ComponentModel.DataAnnotations;
using SpaceManager.Enums;

namespace SpaceManager.DTOs.Payments;

/// <summary>
/// Data transfer object used to record or process a payment against an invoice.
/// </summary>
public record ProcessPaymentDto(
    [Required] int InvoiceId,
    [Range(0.01, 10000000, ErrorMessage = "Payment amount must be greater than zero.")] decimal Amount,
    [Required] PaymentMethod PaymentMethod,
    [Required, MaxLength(100)] string TransactionReference
);

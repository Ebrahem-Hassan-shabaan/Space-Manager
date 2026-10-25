using SpaceManager.Enums;

namespace SpaceManager.DTOs.Payments;

/// <summary>
/// Response projection for payment transactions.
/// </summary>
public record PaymentResponseDto(
    int Id,
    int InvoiceId,
    decimal Amount,
    DateTime PaymentDate,
    PaymentMethod PaymentMethod,
    string TransactionReference
);

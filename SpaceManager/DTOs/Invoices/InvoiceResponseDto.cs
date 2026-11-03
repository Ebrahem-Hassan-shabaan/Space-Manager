using SpaceManager.DTOs.Payments;
using SpaceManager.Enums;
using System.Collections.Generic;

namespace SpaceManager.DTOs.Invoices;

/// <summary>
/// Response projection representing an invoice, with calculated balances and complete payment transaction history.
/// </summary>
public record InvoiceResponseDto(
    int Id,
    decimal Amount,
    DateTime IssuedDate,
    DateTime DueDate,
    InvoiceStatus Status,
    string? UserId,
    string? UserName,
    string? UserEmail,
    int? CompanyId,
    string? CompanyName,
    int? WorkspaceBookingId,
    decimal AmountPaid,
    decimal RemainingBalance,
    List<PaymentResponseDto> Payments
);

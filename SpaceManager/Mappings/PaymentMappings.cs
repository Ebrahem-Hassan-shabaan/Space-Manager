using System;
using SpaceManager.DTOs.Payments;
using SpaceManager.Models;

namespace SpaceManager.Mappings;

/// <summary>
/// Pure static mapping extensions projecting <see cref="Payment"/> entities to strongly typed DTOs and vice versa.
/// </summary>
public static class PaymentMappings
{
    /// <summary>
    /// Projects a <see cref="ProcessPaymentDto"/> payload to a new <see cref="Payment"/> entity timestamped to current UTC.
    /// </summary>
    /// <param name="dto">The payment request data transfer object.</param>
    /// <param name="transactionReference">Optional override for gateway-generated transaction reference.</param>
    /// <returns>A newly instantiated <see cref="Payment"/> ready for persistence.</returns>
    public static Payment ToEntity(this ProcessPaymentDto dto, string? transactionReference = null) =>
        new()
        {
            InvoiceId = dto.InvoiceId,
            Amount = dto.Amount,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = dto.PaymentMethod,
            TransactionReference = transactionReference ?? dto.TransactionReference
        };

    /// <summary>
    /// Projects a <see cref="Payment"/> entity to a <see cref="PaymentResponseDto"/>.
    /// </summary>
    /// <param name="p">The payment entity to project.</param>
    /// <returns>A populated <see cref="PaymentResponseDto"/>.</returns>
    public static PaymentResponseDto ToResponseDto(this Payment p) =>
        new(
            p.Id,
            p.InvoiceId,
            p.Amount,
            p.PaymentDate,
            p.PaymentMethod,
            p.TransactionReference
        );
}

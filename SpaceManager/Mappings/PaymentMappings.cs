using SpaceManager.DTOs.Payments;
using SpaceManager.Models;

namespace SpaceManager.Mappings;

/// <summary>
/// Pure static mapping extensions projecting <see cref="Payment"/> entities to strongly typed DTOs.
/// </summary>
public static class PaymentMappings
{
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

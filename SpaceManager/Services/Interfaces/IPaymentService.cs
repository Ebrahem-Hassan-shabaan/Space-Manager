using SpaceManager.Common;
using SpaceManager.DTOs.Payments;

namespace SpaceManager.Services.Interfaces;

/// <summary>
/// Defines business service operations for processing payments, viewing transaction receipts, and issuing refunds.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Retrieves the platform financial transaction audit log.
    /// (Admins see platform-wide revenue; Company Admins see corporate payments; Members see personal receipts).
    /// </summary>
    /// <param name="caller">The identity context of the authenticated caller.</param>
    /// <returns>A collection of payment transaction receipts matching caller visibility.</returns>
    Task<IEnumerable<PaymentResponseDto>> GetAllAsync(CurrentUserContext caller);

    /// <summary>
    /// Retrieves a specific payment receipt by its unique transaction identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the payment transaction.</param>
    /// <param name="caller">The identity context of the authenticated caller.</param>
    /// <returns>A service result containing the payment receipt DTO if found and authorized.</returns>
    Task<ServiceResult<PaymentResponseDto>> GetByIdAsync(int id, CurrentUserContext caller);

    /// <summary>
    /// Retrieves all payment transactions recorded for a specific invoice (e.g. partial payments, full settlements, and refund entries).
    /// </summary>
    /// <param name="invoiceId">The unique identifier of the target invoice.</param>
    /// <param name="caller">The identity context of the authenticated caller.</param>
    /// <returns>A service result containing the collection of payment receipts for the invoice.</returns>
    Task<ServiceResult<IEnumerable<PaymentResponseDto>>> GetByInvoiceIdAsync(int invoiceId, CurrentUserContext caller);

    /// <summary>
    /// Processes a payment transaction against an invoice.
    /// Supports online card checkout (via mock gateway simulation) and staff-recorded cash or bank transfers.
    /// </summary>
    /// <param name="dto">The payment transaction payload.</param>
    /// <param name="caller">The identity context of the authenticated caller.</param>
    /// <returns>A service result containing the generated payment receipt DTO.</returns>
    Task<ServiceResult<PaymentResponseDto>> ProcessAsync(ProcessPaymentDto dto, CurrentUserContext caller);

    /// <summary>
    /// Issues a refund for a previously settled payment by recording an append-only compensating transaction and updating invoice status.
    /// Restricted to platform administrators.
    /// </summary>
    /// <param name="id">The unique identifier of the payment transaction to refund.</param>
    /// <returns>A service result containing the refund transaction receipt.</returns>
    Task<ServiceResult<PaymentResponseDto>> RefundAsync(int id);
}

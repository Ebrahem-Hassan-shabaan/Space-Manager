using SpaceManager.Common;
using SpaceManager.DTOs.Invoices;

namespace SpaceManager.Services.Interfaces;

/// <summary>
/// Defines business service operations for issuing, retrieving, and managing billing invoices.
/// </summary>
public interface IInvoiceService
{
    /// <summary>
    /// Retrieves a collection of invoices scoped to the caller's role and optional query filters.
    /// (Admins see all; Company Admins see company invoices; Members see their own).
    /// </summary>
    /// <param name="caller">The identity context of the authenticated caller.</param>
    /// <param name="filter">Optional query filters (e.g. status, date range).</param>
    /// <returns>A collection of matching invoice response DTOs.</returns>
    Task<IEnumerable<InvoiceResponseDto>> GetAllAsync(CurrentUserContext caller, InvoiceFilterDto? filter = null);

    /// <summary>
    /// Retrieves a specific invoice by its ID.
    /// Enforces tenant ownership (returns Forbidden if caller does not own the invoice and is not Admin/CompanyAdmin).
    /// </summary>
    /// <param name="id">The unique identifier of the invoice.</param>
    /// <param name="caller">The identity context of the authenticated caller.</param>
    /// <returns>A service result containing the invoice details DTO if authorized.</returns>
    Task<ServiceResult<InvoiceResponseDto>> GetByIdAsync(int id, CurrentUserContext caller);

    /// <summary>
    /// Generates an invoice linked to a newly created workspace booking.
    /// </summary>
    /// <param name="dto">The invoice creation data associated with the booking.</param>
    /// <returns>A service result containing the created invoice response DTO.</returns>
    Task<ServiceResult<InvoiceResponseDto>> CreateForBookingAsync(CreateInvoiceDto dto);

    /// <summary>
    /// Manually issues a standalone or ad-hoc invoice. Restricted to platform administrators.
    /// </summary>
    /// <param name="dto">The manual invoice payload.</param>
    /// <returns>A service result containing the created invoice response DTO.</returns>
    Task<ServiceResult<InvoiceResponseDto>> CreateManualAsync(CreateInvoiceDto dto);

    /// <summary>
    /// Cancels an unpaid invoice. Restricted to platform administrators.
    /// Also releases any linked workspace booking held under PendingPayment.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice to cancel.</param>
    /// <returns>A service result indicating success or failure.</returns>
    Task<ServiceResult> CancelAsync(int id);

    /// <summary>
    /// Automatically cancels the invoice associated with a cancelled workspace booking.
    /// Internal operation called by BookingService.
    /// </summary>
    /// <param name="bookingId">The unique identifier of the cancelled workspace booking.</param>
    /// <returns>A service result indicating whether the invoice was cancelled.</returns>
    Task<ServiceResult> CancelForBookingAsync(int bookingId);
}

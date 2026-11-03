using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpaceManager.Common;
using SpaceManager.Data;
using SpaceManager.DTOs.Notifications;
using SpaceManager.DTOs.Payments;
using SpaceManager.Enums;
using SpaceManager.Mappings;
using SpaceManager.Models;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Services.Implementations;

/// <summary>
/// Implements business logic for payment capture, multi-tenant receipt retrieval,
/// pluggable payment gateway delegation (via Self-Selecting Strategy Pattern),
/// two-phase booking hold confirmation, and accounting-compliant refunds.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly IEnumerable<IPaymentGateway> _gateways;
    private readonly ILogger<PaymentService> _logger;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="gateways">The registered collection of payment gateway strategies.</param>
    /// <param name="logger">The application logger.</param>
    /// <param name="notificationService">The notification engine.</param>
    public PaymentService(
        AppDbContext context,
        IEnumerable<IPaymentGateway> gateways,
        ILogger<PaymentService> logger,
        INotificationService notificationService)
    {
        _context = context;
        _gateways = gateways;
        _logger = logger;
        _notificationService = notificationService;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PaymentResponseDto>> GetAllAsync(CurrentUserContext caller)
    {
        var query = _context.Payments
            .AsNoTracking()
            .Include(p => p.Invoice)
            .AsQueryable();

        if (caller.IsAdmin)
        {
            // Admins see all platform payment transactions
        }
        else if (caller.IsCompanyAdmin && caller.CompanyId.HasValue)
        {
            query = query.Where(p => p.Invoice.CompanyId == caller.CompanyId.Value);
        }
        else if (!string.IsNullOrEmpty(caller.UserId))
        {
            query = query.Where(p => p.Invoice.UserId == caller.UserId);
        }
        else
        {
            return Enumerable.Empty<PaymentResponseDto>();
        }

        var payments = await query
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return payments.Select(p => p.ToResponseDto());
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PaymentResponseDto>> GetByIdAsync(int id, CurrentUserContext caller)
    {
        var payment = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null)
            return ServiceResult<PaymentResponseDto>.Failure("Payment transaction not found.");

        var isAuthorized = caller.IsAdmin
            || (caller.IsCompanyAdmin && caller.CompanyId.HasValue && payment.Invoice.CompanyId == caller.CompanyId.Value)
            || (!string.IsNullOrEmpty(caller.UserId) && payment.Invoice.UserId == caller.UserId);

        if (!isAuthorized)
            return ServiceResult<PaymentResponseDto>.Forbidden("You are not authorized to view this payment receipt.");

        return ServiceResult<PaymentResponseDto>.Success(payment.ToResponseDto());
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<PaymentResponseDto>>> GetByInvoiceIdAsync(int invoiceId, CurrentUserContext caller)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
            return ServiceResult<IEnumerable<PaymentResponseDto>>.Failure("Invoice not found.");

        var isAuthorized = caller.IsAdmin
            || (caller.IsCompanyAdmin && caller.CompanyId.HasValue && invoice.CompanyId == caller.CompanyId.Value)
            || (!string.IsNullOrEmpty(caller.UserId) && invoice.UserId == caller.UserId);

        if (!isAuthorized)
            return ServiceResult<IEnumerable<PaymentResponseDto>>.Forbidden("You are not authorized to view payments for this invoice.");

        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p => p.InvoiceId == invoiceId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return ServiceResult<IEnumerable<PaymentResponseDto>>.Success(payments.Select(p => p.ToResponseDto()));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PaymentResponseDto>> ProcessAsync(ProcessPaymentDto dto, CurrentUserContext caller)
    {
        if (dto.Amount <= 0)
            return ServiceResult<PaymentResponseDto>.Failure("Payment amount must be greater than zero.");

        // Rule: Cash payments are strictly restricted to Administrators
        if (dto.PaymentMethod == PaymentMethod.Cash && !caller.IsAdmin)
        {
            return ServiceResult<PaymentResponseDto>.Forbidden("Cash payments can only be recorded by administrators.");
        }

        // Rule: CompanyBalance payments require affiliation with a corporate account
        if (dto.PaymentMethod == PaymentMethod.CompanyBalance && !caller.CompanyId.HasValue && !caller.IsCompanyAdmin)
        {
            return ServiceResult<PaymentResponseDto>.Forbidden("Corporate Balance payment is only available for accounts affiliated with a corporate organization.");
        }

        // Rule: Credit and Debit Card payments require card credentials or a payment token
        if ((dto.PaymentMethod == PaymentMethod.CreditCard || dto.PaymentMethod == PaymentMethod.DebitCard)
            && string.IsNullOrWhiteSpace(dto.TransactionReference))
        {
            return ServiceResult<PaymentResponseDto>.Failure("A valid card number or payment token is required to process card transactions.");
        }

        var invoice = await _context.Invoices
            .Include(i => i.Payments)
            .Include(i => i.WorkspaceBooking)
            .FirstOrDefaultAsync(i => i.Id == dto.InvoiceId);

        if (invoice == null)
            return ServiceResult<PaymentResponseDto>.Failure("Invoice not found.");

        // Verify caller ownership
        var isAuthorized = caller.IsAdmin
            || (caller.IsCompanyAdmin && caller.CompanyId.HasValue && invoice.CompanyId == caller.CompanyId.Value)
            || (!string.IsNullOrEmpty(caller.UserId) && invoice.UserId == caller.UserId);

        if (!isAuthorized)
            return ServiceResult<PaymentResponseDto>.Forbidden("You are not authorized to make payments against this invoice.");

        // Invariant checks on invoice status
        if (invoice.Status == InvoiceStatus.Cancelled)
            return ServiceResult<PaymentResponseDto>.Failure("Cannot process payments against a cancelled invoice.");

        if (invoice.Status == InvoiceStatus.Paid)
            return ServiceResult<PaymentResponseDto>.Failure("Invoice is already fully paid.");

        if (invoice.Status == InvoiceStatus.Refunded)
            return ServiceResult<PaymentResponseDto>.Failure("Cannot process payments against a refunded invoice.");

        // Prevent overpayment
        var existingPaid = invoice.Payments.Sum(p => p.Amount);
        var remainingBalance = invoice.Amount - existingPaid;
        if (dto.Amount > remainingBalance)
        {
            return ServiceResult<PaymentResponseDto>.Failure($"Payment amount ({dto.Amount:C}) exceeds the remaining invoice balance ({remainingBalance:C}).");
        }

        // Self-Selecting Strategy: Delegate to an external payment gateway if one handles this method
        string settledTransactionReference;
        var gateway = _gateways.FirstOrDefault(g => g.CanHandle(dto.PaymentMethod));

        if (gateway != null)
        {
            var gatewayRequest = new PaymentGatewayRequest(
                Amount: dto.Amount,
                Currency: "EGP",
                PaymentMethod: dto.PaymentMethod,
                PaymentToken: dto.TransactionReference,
                CustomerEmail: invoice.User?.Email,
                Description: $"Invoice #{invoice.Id} settlement",
                Metadata: new Dictionary<string, string>
                {
                    { "InvoiceId", invoice.Id.ToString() },
                    { "UserId", invoice.UserId ?? string.Empty }
                }
            );

            var gatewayResult = await gateway.ChargeAsync(gatewayRequest);
            if (!gatewayResult.IsSuccess)
            {
                _logger.LogWarning("Payment gateway declined transaction for invoice #{InvoiceId}. Reason: {Error} ({Code})",
                    dto.InvoiceId, gatewayResult.ErrorMessage, gatewayResult.ErrorCode);

                return ServiceResult<PaymentResponseDto>.Failure(gatewayResult.ErrorMessage ?? "Payment gateway declined the transaction.");
            }

            settledTransactionReference = gatewayResult.TransactionReference!;
        }
        else
        {
            // Direct/Manual method (e.g. Cash recorded by Admin, or direct BankTransfer receipt)
            settledTransactionReference = string.IsNullOrWhiteSpace(dto.TransactionReference)
                ? $"{dto.PaymentMethod.ToString().ToUpperInvariant()}-{Guid.NewGuid():N}"[..16].ToUpperInvariant()
                : dto.TransactionReference;
        }

        // Persist payment entity using mapping extension
        var payment = dto.ToEntity(settledTransactionReference);
        _context.Payments.Add(payment);

        // Update invoice and linked booking status
        var totalPaid = existingPaid + dto.Amount;
        if (totalPaid >= invoice.Amount)
        {
            invoice.Status = InvoiceStatus.Paid;

            // Two-Phase Hold Confirmation: transition PendingPayment booking to Confirmed
            if (invoice.WorkspaceBooking != null && invoice.WorkspaceBooking.Status == BookingStatus.PendingPayment)
            {
                invoice.WorkspaceBooking.Status = BookingStatus.Confirmed;
                _logger.LogInformation("Booking #{BookingId} confirmed via full invoice payment #{InvoiceId}.", invoice.WorkspaceBookingId, invoice.Id);
            }
        }
        else if (totalPaid > 0)
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }

        await _context.SaveChangesAsync();

        // Dispatch notifications to the payer
        if (!string.IsNullOrEmpty(invoice.UserId))
        {
            if (invoice.WorkspaceBooking != null && invoice.WorkspaceBooking.Status == BookingStatus.Confirmed)
            {
                await _notificationService.SendNotificationAsync(new CreateNotificationDto
                {
                    UserId = invoice.UserId,
                    Title = "Booking Confirmed",
                    Message = $"Your payment of {payment.Amount:0.00} EGP was received. Reservation #{invoice.WorkspaceBookingId} is now confirmed!",
                    ActionUrl = "/bookings/my-bookings",
                    WorkspaceBookingId = invoice.WorkspaceBookingId
                });
            }
            else
            {
                await _notificationService.SendNotificationAsync(new CreateNotificationDto
                {
                    UserId = invoice.UserId,
                    Title = "Payment Successful",
                    Message = $"Payment of {payment.Amount:0.00} EGP for Invoice #{invoice.Id} was successfully processed.",
                    ActionUrl = "/invoices"
                });
            }
        }

        _logger.LogInformation("Recorded payment of {Amount:C} via {Method} for invoice #{InvoiceId}. New invoice status: {Status}.",
            payment.Amount, payment.PaymentMethod, invoice.Id, invoice.Status);

        return ServiceResult<PaymentResponseDto>.Success(payment.ToResponseDto());
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PaymentResponseDto>> RefundAsync(int id)
    {
        var originalPayment = await _context.Payments
            .Include(p => p.Invoice)
                .ThenInclude(i => i.WorkspaceBooking)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (originalPayment == null)
            return ServiceResult<PaymentResponseDto>.Failure("Payment transaction not found.");

        if (originalPayment.Amount <= 0)
            return ServiceResult<PaymentResponseDto>.Failure("Cannot refund a negative or zero-amount transaction.");

        // Check if this specific transaction has already been refunded
        var alreadyRefunded = await _context.Payments
            .AnyAsync(p => p.TransactionReference == $"REF-{originalPayment.TransactionReference}" || p.TransactionReference == $"REF-{originalPayment.Id}");

        if (alreadyRefunded)
            return ServiceResult<PaymentResponseDto>.Failure("This payment transaction has already been refunded.");

        // Self-Selecting Strategy: Delegate to gateway refund if one handles this method
        string refundReference;
        var gateway = _gateways.FirstOrDefault(g => g.CanHandle(originalPayment.PaymentMethod));

        if (gateway != null)
        {
            var gatewayRefundResult = await gateway.RefundAsync(originalPayment.TransactionReference, originalPayment.Amount);
            if (!gatewayRefundResult.IsSuccess)
            {
                _logger.LogWarning("Payment gateway refund failed for transaction #{OriginalId}: {Error}",
                    originalPayment.Id, gatewayRefundResult.ErrorMessage);

                return ServiceResult<PaymentResponseDto>.Failure(gatewayRefundResult.ErrorMessage ?? "Payment gateway refund failed.");
            }

            refundReference = gatewayRefundResult.TransactionReference!;
        }
        else
        {
            refundReference = $"REF-{originalPayment.TransactionReference}";
        }

        // Record accounting-compliant compensating negative transaction
        var refundPayment = new Payment
        {
            InvoiceId = originalPayment.InvoiceId,
            Amount = -originalPayment.Amount,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = originalPayment.PaymentMethod,
            TransactionReference = refundReference
        };

        _context.Payments.Add(refundPayment);

        // Recalculate invoice net payments
        var allPayments = await _context.Payments
            .Where(p => p.InvoiceId == originalPayment.InvoiceId)
            .ToListAsync();

        var netPaid = allPayments.Sum(p => p.Amount) - originalPayment.Amount;

        if (netPaid <= 0)
        {
            originalPayment.Invoice.Status = InvoiceStatus.Refunded;

            // If linked to a workspace booking, cancel the booking
            if (originalPayment.Invoice.WorkspaceBooking != null && originalPayment.Invoice.WorkspaceBooking.Status != BookingStatus.Cancelled)
            {
                originalPayment.Invoice.WorkspaceBooking.Status = BookingStatus.Cancelled;
                _logger.LogInformation("Booking #{BookingId} cancelled due to refund of invoice #{InvoiceId}.",
                    originalPayment.Invoice.WorkspaceBookingId, originalPayment.InvoiceId);
            }
        }
        else
        {
            originalPayment.Invoice.Status = InvoiceStatus.PartiallyPaid;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Issued refund payment of {Amount:C} for transaction #{OriginalId}. Invoice #{InvoiceId} status: {Status}.",
            refundPayment.Amount, originalPayment.Id, originalPayment.InvoiceId, originalPayment.Invoice.Status);

        return ServiceResult<PaymentResponseDto>.Success(refundPayment.ToResponseDto());
    }
}

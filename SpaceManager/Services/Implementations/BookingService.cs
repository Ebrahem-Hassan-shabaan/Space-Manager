using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpaceManager.Data;
using SpaceManager.DTOs.Bookings;
using SpaceManager.DTOs.Invoices;
using SpaceManager.DTOs.Notifications;
using SpaceManager.DTOs.ServiceAddOns;
using SpaceManager.Enums;
using SpaceManager.Models;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Services.Implementations;

/// <summary>
/// Implements workspace scheduling, availability validation, two-phase reservation hold management,
/// and automated invoice generation for both corporate accounts and individual retail clients.
/// </summary>
public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private readonly IAddOnService _addOnservice;
    private readonly IInvoiceService _invoiceService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookingService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="addOnService">The add-on service for calculating ancillary costs.</param>
    /// <param name="invoiceService">The invoice service for managing reservation billing.</param>
    /// <param name="subscriptionService">The subscription service for evaluating active memberships.</param>
    /// <param name="notificationService">The notification engine.</param>
    public BookingService(
        AppDbContext context,
        IAddOnService addOnService,
        IInvoiceService invoiceService,
        ISubscriptionService subscriptionService,
        INotificationService notificationService)
    {
        _addOnservice = addOnService;
        _context = context;
        _invoiceService = invoiceService;
        _subscriptionService = subscriptionService;
        _notificationService = notificationService;
    }

    /// <inheritdoc />
    public decimal CalculateTotalCost(DateTime startTime, DateTime endTime, decimal pricePerHour)
    {
        var duration = endTime - startTime;
        var billableHalfHours = (decimal)Math.Ceiling(duration.TotalMinutes / 30.0);
        return Math.Round(billableHalfHours * (pricePerHour / 2m), 2);
    }

    /// <inheritdoc />
    public async Task<int> SealExpiredBookingsAsync()
    {
        var now = DateTime.UtcNow;
        var expiredBookings = await _context.WorkspaceBookings
            .Include(b => b.Workspace)
            .Where(b => b.Status == BookingStatus.Confirmed && b.EndTime < now)
            .ToListAsync();

        if (expiredBookings.Count == 0) return 0;

        foreach (var booking in expiredBookings)
        {
            booking.Status = BookingStatus.Completed;
            await _notificationService.SendNotificationAsync(new CreateNotificationDto
            {
                UserId = booking.UserId,
                Title = "Leave a Review",
                Message = $"How was your visit to {booking.Workspace?.Name ?? "the workspace"}? Share your experience with other members.",
                ActionUrl = "/bookings/my-bookings",
                WorkspaceBookingId = booking.Id
            });
        }

        await _context.SaveChangesAsync();
        return expiredBookings.Count;
    }

    /// <inheritdoc />
    public async Task<int> ReleaseExpiredHoldsAsync()
    {
        var threshold = DateTime.UtcNow.AddMinutes(-15);
        var expiredHolds = await _context.WorkspaceBookings
            .Where(b => b.Status == BookingStatus.PendingPayment && b.CreatedAt < threshold)
            .ToListAsync();

        if (expiredHolds.Count == 0) return 0;

        foreach (var booking in expiredHolds)
        {
            booking.Status = BookingStatus.Cancelled;
            await _invoiceService.CancelForBookingAsync(booking.Id);
        }

        await _context.SaveChangesAsync();
        return expiredHolds.Count;
    }

    /// <inheritdoc />
    public async Task<BookingResponseDto> CreateBookingAsync(string userId, CreateBookingDto dto)
    {
        await ReleaseExpiredHoldsAsync();

        var workspace = await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == dto.WorkspaceId);
        if (workspace == null)
        {
            throw new ArgumentException("Workspace not found.");
        }

        if (!workspace.IsAvailable)
        {
            throw new InvalidOperationException("Workspace is not available for booking.");
        }

        if (dto.StartTime >= dto.EndTime)
        {
            throw new ArgumentException("Start time must be before end time.");
        }

        if (dto.StartTime < DateTime.UtcNow)
        {
            throw new ArgumentException("Start time must be in the future.");
        }

        var holdThreshold = DateTime.UtcNow.AddMinutes(-15);
        var overlappingSlots = await _context.WorkspaceBookings
            .AsNoTracking()
            .Where(b =>
                b.WorkspaceId == dto.WorkspaceId &&
                dto.StartTime < b.EndTime &&
                dto.EndTime > b.StartTime &&
                (b.Status == BookingStatus.Confirmed ||
                 (b.Status == BookingStatus.PendingPayment && b.CreatedAt >= holdThreshold)))
            .OrderBy(b => b.StartTime)
            .Select(b => new ConflictingSlotDto(b.StartTime, b.EndTime))
            .ToListAsync();

        if (overlappingSlots.Count > 0)
        {
            var conflictDetails = new BookingConflictResponseDto(
                Message: "The workspace is already booked for the selected time range.",
                WorkspaceId: dto.WorkspaceId,
                RequestedStartTime: dto.StartTime,
                RequestedEndTime: dto.EndTime,
                ConflictingSlots: overlappingSlots
            );
            throw new BookingConflictException(conflictDetails);
        }

        decimal roomCost = CalculateTotalCost(dto.StartTime, dto.EndTime, workspace.PricePerHour);
        decimal addOnsCost = await _addOnservice.CalculateTotalCostAsync(dto.BookingAddons);

        // Evaluate active membership privileges
        var subResult = await _subscriptionService.GetActiveSubscriptionForUserAsync(userId);
        if (subResult.Succeeded && subResult.Data != null)
        {
            var plan = subResult.Data;
            bool coversWorkspace = plan.IsAllAccess || plan.WorkspaceTypeId == workspace.WorkspaceTypeId;
            bool withinQuota = plan.MaxBookingsPerMonth == 0 || plan.BookingsUsedThisMonth < plan.MaxBookingsPerMonth;

            if (coversWorkspace && withinQuota)
            {
                roomCost = 0m; // Hourly room rate 100% covered by membership!
            }
        }

        decimal grandCost = roomCost + addOnsCost;

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        bool isCorporate = user?.CompanyId.HasValue == true;

        // Rule: Only bookings with zero remaining cost (e.g. room 100% covered by membership with no add-ons) confirm immediately.
        // If there are billable items (room cost or add-ons), the booking is held in PendingPayment pending checkout payment.
        var bookingStatus = grandCost == 0m
            ? BookingStatus.Confirmed
            : BookingStatus.PendingPayment;

        var workspaceBooking = new WorkspaceBooking
        {
            UserId = userId,
            WorkspaceId = dto.WorkspaceId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            TotalCost = grandCost,
            Status = bookingStatus
        };

        var serviceMappings = new Dictionary<int, ServiceAddOn>();

        if (dto.BookingAddons != null && dto.BookingAddons.Any())
        {
            var addOnIds = dto.BookingAddons.Select(a => a.ServiceAddOnId).Distinct().ToList();
            serviceMappings = await _context.ServiceAddOns
                .Where(sa => addOnIds.Contains(sa.Id))
                .ToDictionaryAsync(a => a.Id);

            foreach (var item in dto.BookingAddons)
            {
                var serviceAddOn = serviceMappings[item.ServiceAddOnId];
                workspaceBooking.BookingAddOns.Add(new BookingAddOn
                {
                    ServiceAddOnId = item.ServiceAddOnId,
                    Quantity = item.Quantity,
                    TotalPrice = item.Quantity * serviceAddOn.Price
                });
            }
        }

        _context.WorkspaceBookings.Add(workspaceBooking);
        await _context.SaveChangesAsync();

        // Issue linked invoice when costs remain (e.g. retail booking, corporate Net-30, or subscription add-ons)
        int? invoiceId = null;
        if (workspaceBooking.TotalCost > 0m)
        {
            var invoiceResult = await _invoiceService.CreateForBookingAsync(new CreateInvoiceDto(
                Amount: workspaceBooking.TotalCost,
                DueDate: isCorporate ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddMinutes(15),
                UserId: userId,
                CompanyId: user?.CompanyId,
                WorkspaceBookingId: workspaceBooking.Id
            ));

            if (invoiceResult.Succeeded && invoiceResult.Data != null)
            {
                invoiceId = invoiceResult.Data.Id;
            }
        }

        // Notify member immediately if booking was confirmed right away
        if (workspaceBooking.Status == BookingStatus.Confirmed)
        {
            await _notificationService.SendNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Title = "Booking Confirmed",
                Message = $"Your reservation for {workspace.Name} on {workspaceBooking.StartTime:MMM dd, yyyy HH:mm} is confirmed.",
                ActionUrl = "/bookings/my-bookings",
                WorkspaceBookingId = workspaceBooking.Id
            });
        }

        var responseAddOns = workspaceBooking.BookingAddOns
            .Select(ba => new BookingAddOnResponseDto(
                ba.ServiceAddOnId,
                ba.ServiceAddOn?.Name ?? string.Empty,
                ba.Quantity > 0 ? (ba.TotalPrice / ba.Quantity) : 0m,
                ba.Quantity,
                ba.TotalPrice
            ))
            .ToList();

        return new BookingResponseDto(
            workspaceBooking.Id,
            workspaceBooking.WorkspaceId,
            workspace.Name,
            workspaceBooking.StartTime,
            workspaceBooking.EndTime,
            workspaceBooking.TotalCost,
            workspaceBooking.Status,
            responseAddOns,
            invoiceId
        );
    }

    /// <inheritdoc />
    public async Task<bool> CancelBookingAsync(int bookingId, string? userId = null)
    {
        var query = _context.WorkspaceBookings.Where(b => b.Id == bookingId);
        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(b => b.UserId == userId);
        }
        var booking = await query.FirstOrDefaultAsync();

        if (booking == null)
        {
            return false;
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("This booking is already cancelled.");
        }
        if (booking.StartTime <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Cannot cancel a booking that has already started or passed.");
        }
        else if (booking.StartTime - DateTime.UtcNow < TimeSpan.FromHours(1))
        {
            throw new InvalidOperationException("Bookings can only be cancelled at least 1 hour before start time.");
        }

        booking.Status = BookingStatus.Cancelled;
        await _context.SaveChangesAsync();

        // Void any linked unpaid invoice automatically
        await _invoiceService.CancelForBookingAsync(bookingId);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ReleaseHoldAsync(int bookingId, string? userId = null)
    {
        var query = _context.WorkspaceBookings.Where(b => b.Id == bookingId && b.Status == BookingStatus.PendingPayment);
        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(b => b.UserId == userId);
        }

        var booking = await query.FirstOrDefaultAsync();
        if (booking == null)
        {
            return false;
        }

        booking.Status = BookingStatus.Cancelled;
        await _context.SaveChangesAsync();

        // Void the draft invoice linked to this hold
        await _invoiceService.CancelForBookingAsync(bookingId);

        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(string userId, bool? upcoming = null)
    {
        await SealExpiredBookingsAsync();
        await ReleaseExpiredHoldsAsync();

        // Approach B: Exclude unconfirmed ephemeral PendingPayment holds from customer's booking view
        var query = _context.WorkspaceBookings
            .AsNoTracking()
            .Where(b => b.UserId == userId && b.Status != BookingStatus.PendingPayment);

        if (upcoming.HasValue)
        {
            query = upcoming.Value
                ? query.Where(b => b.StartTime > DateTime.UtcNow).OrderBy(b => b.StartTime)
                : query.Where(b => b.StartTime <= DateTime.UtcNow).OrderByDescending(b => b.StartTime);
        }
        else
        {
            query = query.OrderByDescending(b => b.StartTime);
        }

        return await query.Select(b => new BookingResponseDto(
            b.Id,
            b.WorkspaceId,
            b.Workspace.Name,
            b.StartTime,
            b.EndTime,
            b.TotalCost,
            b.Status,
            b.BookingAddOns.Select(ba => new BookingAddOnResponseDto(
                ba.ServiceAddOnId,
                ba.ServiceAddOn.Name,
                ba.Quantity > 0 ? (ba.TotalPrice / ba.Quantity) : ba.ServiceAddOn.Price,
                ba.Quantity,
                ba.TotalPrice
            )).ToList(),
            b.Invoice != null ? (int?)b.Invoice.Id : null
        )).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync(bool? upcoming = null)
    {
        await SealExpiredBookingsAsync();
        await ReleaseExpiredHoldsAsync();

        var query = _context.WorkspaceBookings
            .AsNoTracking();

        if (upcoming.HasValue)
        {
            query = upcoming.Value
                ? query.Where(b => b.StartTime > DateTime.UtcNow).OrderBy(b => b.StartTime)
                : query.Where(b => b.StartTime <= DateTime.UtcNow).OrderByDescending(b => b.StartTime);
        }
        else
        {
            query = query.OrderByDescending(b => b.StartTime);
        }

        return await query.Select(b => new BookingResponseDto(
            b.Id,
            b.WorkspaceId,
            b.Workspace.Name,
            b.StartTime,
            b.EndTime,
            b.TotalCost,
            b.Status,
            b.BookingAddOns.Select(ba => new BookingAddOnResponseDto(
                ba.ServiceAddOnId,
                ba.ServiceAddOn.Name,
                ba.Quantity > 0 ? (ba.TotalPrice / ba.Quantity) : ba.ServiceAddOn.Price,
                ba.Quantity,
                ba.TotalPrice
            )).ToList(),
            b.Invoice != null ? (int?)b.Invoice.Id : null
        )).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<BookingResponseDto?> GetBookingByIdAsync(int bookingId, string? userId = null)
    {
        var query = _context.WorkspaceBookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId);

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(b => b.UserId == userId);
        }

        return await query.Select(b => new BookingResponseDto(
            b.Id,
            b.WorkspaceId,
            b.Workspace.Name,
            b.StartTime,
            b.EndTime,
            b.TotalCost,
            b.Status,
            b.BookingAddOns.Select(ba => new BookingAddOnResponseDto(
                ba.ServiceAddOnId,
                ba.ServiceAddOn.Name,
                ba.Quantity > 0 ? (ba.TotalPrice / ba.Quantity) : ba.ServiceAddOn.Price,
                ba.Quantity,
                ba.TotalPrice
            )).ToList(),
            b.Invoice != null ? (int?)b.Invoice.Id : null
        )).FirstOrDefaultAsync();
    }
}

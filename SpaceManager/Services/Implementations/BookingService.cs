using Microsoft.EntityFrameworkCore;
using SpaceManager.Data;
using SpaceManager.DTOs.Bookings;
using SpaceManager.DTOs.ServiceAddOns;
using SpaceManager.Enums;
using SpaceManager.Models;
using SpaceManager.Services.Interfaces;
using System.Linq;

namespace SpaceManager.Services.Implementations;

/// <summary>
/// Implements workspace scheduling, availability validation, and reservation management.
/// </summary>
public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private readonly IAddOnService _addOnservice;

    public BookingService(AppDbContext context, IAddOnService addOnService)
    {
        _addOnservice = addOnService;
        _context = context;
    }

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
            .Where(b => b.Status == BookingStatus.Confirmed && b.EndTime < now)
            .ToListAsync();

        if (expiredBookings.Count == 0) return 0;

        foreach (var booking in expiredBookings)
            booking.Status = BookingStatus.Completed;

        await _context.SaveChangesAsync();
        return expiredBookings.Count;
    }

    public async Task<BookingResponseDto> CreateBookingAsync(string userId, CreateBookingDto dto)
    {
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

        var overlappingSlots = await _context.WorkspaceBookings
            .AsNoTracking()
            .Where(b =>
                b.WorkspaceId == dto.WorkspaceId &&
                b.Status == BookingStatus.Confirmed &&
                dto.StartTime < b.EndTime &&
                dto.EndTime > b.StartTime)
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
        decimal grandCost = roomCost + addOnsCost;

        var workspaceBooking = new WorkspaceBooking
        {
            UserId = userId,
            WorkspaceId = dto.WorkspaceId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            TotalCost = grandCost,
            Status = BookingStatus.Confirmed
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
            responseAddOns
        );
    }

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
        return true;
    }

    public async Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(string userId, bool? upcoming = null)
    {
        await SealExpiredBookingsAsync();

        var query = _context.WorkspaceBookings
            .AsNoTracking()
            .Where(b => b.UserId == userId);

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
            )).ToList()
        )).ToListAsync();
    }

    public async Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync(bool? upcoming = null)
    {
        await SealExpiredBookingsAsync();

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
            )).ToList()
        )).ToListAsync();
    }

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
            )).ToList()
        )).FirstOrDefaultAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using SpaceManager.Data;
using SpaceManager.DTOs.Bookings;
using SpaceManager.Enums;
using SpaceManager.Models;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Services.Implementations;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;

    public BookingService(AppDbContext context)
    {
        _context = context;
    }

    private decimal CalculateTotalCost(DateTime startTime, DateTime endTime, decimal pricePerHour)
    {
        var duration = endTime - startTime;
        var billableHalfHours = (decimal)Math.Ceiling(duration.TotalMinutes / 30.0);
        return Math.Round(billableHalfHours * (pricePerHour / 2m), 2);
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

        var hasOverlap = await _context.WorkspaceBookings.AnyAsync(b =>
            b.WorkspaceId == dto.WorkspaceId &&
            b.Status == BookingStatus.Confirmed &&
            dto.StartTime < b.EndTime &&
            dto.EndTime > b.StartTime
        );

        if (hasOverlap)
        {
            throw new InvalidOperationException("The workspace is already booked for the selected time range.");
        }

        decimal totalCost = CalculateTotalCost(dto.StartTime, dto.EndTime, workspace.PricePerHour);

        var workspaceBooking = new WorkspaceBooking
        {
            UserId = userId,
            WorkspaceId = dto.WorkspaceId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            TotalCost = totalCost,
            Status = BookingStatus.Confirmed
        };

        _context.WorkspaceBookings.Add(workspaceBooking);
        await _context.SaveChangesAsync();

        return new BookingResponseDto(
            workspaceBooking.Id,
            workspaceBooking.WorkspaceId,
            workspace.Name,
            workspaceBooking.StartTime,
            workspaceBooking.EndTime,
            workspaceBooking.TotalCost,
            workspaceBooking.Status
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
        // e.g. Must cancel at least 1 hour before StartTime
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
            b.Status
        )).ToListAsync();
    }

    public async Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync(bool? upcoming = null)
    {
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
            b.Status
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
            b.Status
        )).FirstOrDefaultAsync();
    }
}
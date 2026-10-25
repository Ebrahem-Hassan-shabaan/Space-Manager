using Microsoft.EntityFrameworkCore;
using SpaceManager.Data;
using SpaceManager.DTOs.VisitorPasses;
using SpaceManager.Enums;
using SpaceManager.Models;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Services.Implementations;

/// <summary>
/// Implements visitor pass generation, verification, and reception check-in logging.
/// </summary>
public class VisitorPassService : IVisitorPassService
{
    private readonly AppDbContext _context;

    public VisitorPassService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CheckInVisitorPassAsync(int visitorPassId)
    {
        var visitorPass = await _context.VisitorPasses
            .Include(vp => vp.WorkspaceBooking)
            .FirstOrDefaultAsync(vp => vp.Id == visitorPassId);

        if (visitorPass == null)
        {
            return false;
        }

        if (visitorPass.WorkspaceBooking.Status == BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot check in a visitor for a cancelled booking.");
        }

        if (visitorPass.ActualCheckInTime != null)
        {
            throw new InvalidOperationException("Visitor is already checked in.");
        }

        var nowUtc = DateTime.UtcNow;
        var expectedArrivalUtc = visitorPass.ExpectedArrival.Kind == DateTimeKind.Utc
            ? visitorPass.ExpectedArrival
            : DateTime.SpecifyKind(visitorPass.ExpectedArrival, DateTimeKind.Utc);
        var bookingEndUtc = visitorPass.WorkspaceBooking.EndTime.Kind == DateTimeKind.Utc
            ? visitorPass.WorkspaceBooking.EndTime
            : DateTime.SpecifyKind(visitorPass.WorkspaceBooking.EndTime, DateTimeKind.Utc);

        if (nowUtc < expectedArrivalUtc)
        {
            throw new InvalidOperationException("Cannot check in the visitor before their expected arrival time.");
        }

        if (nowUtc > bookingEndUtc)
        {
            throw new InvalidOperationException("Cannot check in the visitor because the workspace booking has ended.");
        }

        visitorPass.ActualCheckInTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<VisitorPassResponseDto> CreateVisitorPassAsync(string userId, CreateVisitorPassDto dto)
    {
        var booking = await _context.WorkspaceBookings
            .FirstOrDefaultAsync(b => b.Id == dto.WorkspaceBookingId && b.UserId == userId);

        if (booking == null)
        {
            throw new ArgumentException($"Workspace booking with ID {dto.WorkspaceBookingId} was not found for this user.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot create a visitor pass for a cancelled booking.");
        }

        if (dto.ExpectedArrival < booking.StartTime || dto.ExpectedArrival > booking.EndTime)
        {
            throw new ArgumentException("Expected arrival time must fall within the booking time slot.");
        }

        var visitorPass = new VisitorPass
        {
            WorkspaceBookingId = dto.WorkspaceBookingId,
            GuestName = dto.GuestName,
            GuestEmail = dto.GuestEmail,
            ExpectedArrival = dto.ExpectedArrival,
            UserId = userId
        };

        _context.VisitorPasses.Add(visitorPass);
        await _context.SaveChangesAsync();

        return await GetVisitorPassByIdAsync(visitorPass.Id, userId)
            ?? throw new InvalidOperationException("Failed to retrieve the created visitor pass.");
    }

    public async Task<bool> DeleteVisitorPassAsync(int visitorPassId, string? userId = null)
    {
        var query = _context.VisitorPasses.Where(vp => vp.Id == visitorPassId);

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(vp => vp.UserId == userId);
        }

        var visitorPass = await query.FirstOrDefaultAsync();
        if (visitorPass == null)
        {
            return false;
        }

        if (visitorPass.ActualCheckInTime != null)
        {
            throw new InvalidOperationException("Cannot delete a visitor pass for a guest who has already checked in.");
        }

        _context.VisitorPasses.Remove(visitorPass);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<VisitorPassResponseDto>> GetAllVisitorPassesAsync()
    {
        return await _context.VisitorPasses
            .AsNoTracking()
            .OrderByDescending(vp => vp.ExpectedArrival)
            .Select(vp => new VisitorPassResponseDto(
                vp.Id,
                vp.WorkspaceBookingId,
                vp.WorkspaceBooking.Workspace.Name,
                vp.UserId,
                vp.User.FullName,
                vp.GuestName,
                vp.GuestEmail,
                vp.ExpectedArrival,
                vp.ActualCheckInTime,
                vp.ActualCheckInTime.HasValue,
                vp.WorkspaceBooking.EndTime,
                vp.WorkspaceBooking.StartTime
            ))
            .ToListAsync();
    }

    public async Task<IEnumerable<VisitorPassResponseDto>> GetUserVisitorPassesAsync(string userId)
    {
        return await _context.VisitorPasses
            .AsNoTracking()
            .Where(vp => vp.UserId == userId)
            .OrderByDescending(vp => vp.ExpectedArrival)
            .Select(vp => new VisitorPassResponseDto(
                vp.Id,
                vp.WorkspaceBookingId,
                vp.WorkspaceBooking.Workspace.Name,
                vp.UserId,
                vp.User.FullName,
                vp.GuestName,
                vp.GuestEmail,
                vp.ExpectedArrival,
                vp.ActualCheckInTime,
                vp.ActualCheckInTime.HasValue,
                vp.WorkspaceBooking.EndTime,
                vp.WorkspaceBooking.StartTime
            ))
            .ToListAsync();
    }

    public async Task<VisitorPassResponseDto?> GetVisitorPassByIdAsync(int visitorPassId, string? userId = null)
    {
        var query = _context.VisitorPasses
            .AsNoTracking()
            .Where(vp => vp.Id == visitorPassId);

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(vp => vp.UserId == userId);
        }

        return await query
            .Select(vp => new VisitorPassResponseDto(
                vp.Id,
                vp.WorkspaceBookingId,
                vp.WorkspaceBooking.Workspace.Name,
                vp.UserId,
                vp.User.FullName,
                vp.GuestName,
                vp.GuestEmail,
                vp.ExpectedArrival,
                vp.ActualCheckInTime,
                vp.ActualCheckInTime.HasValue,
                vp.WorkspaceBooking.EndTime,
                vp.WorkspaceBooking.StartTime
            ))
            .FirstOrDefaultAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using SpaceManager.Data;
using SpaceManager.DTOs.Tickets;
using SpaceManager.Enums;
using SpaceManager.Models;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Services.Implementations;

public class MaintenanceTicketService : IMaintenanceTicketService
{
    private readonly AppDbContext _context;

    public MaintenanceTicketService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TicketResponseDto> CreateMaintenanceTicketAsync(string userId, CreateTicketDto dto)
    {
        var workspaceExists = await _context.Workspaces.AnyAsync(w => w.Id == dto.WorkspaceId);
        if (!workspaceExists)
        {
            throw new ArgumentException($"Workspace with ID {dto.WorkspaceId} does not exist.");
        }

        var ticket = new MaintenanceTicket
        {
            UserId = userId,
            WorkspaceId = dto.WorkspaceId,
            IssueTitle = dto.IssueTitle,
            Description = dto.Description,
            EmergencyLevel = dto.EmergencyLevel,
            Status = TicketStatus.Open,
            ReportedAt = DateTime.UtcNow
        };

        _context.MaintenanceTickets.Add(ticket);
        await _context.SaveChangesAsync();

        return await GetMaintenanceTicketByIdAsync(ticket.Id)
            ?? throw new InvalidOperationException("Failed to retrieve the created maintenance ticket.");
    }

    public async Task<IEnumerable<TicketResponseDto>> GetAllMaintenanceTicketsAsync(
        TicketStatus? status = null,
        TicketEmergencyLevel? emergencyLevel = null)
    {
        var query = _context.MaintenanceTickets.AsNoTracking().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (emergencyLevel.HasValue)
        {
            query = query.Where(t => t.EmergencyLevel == emergencyLevel.Value);
        }

        return await query
            .OrderByDescending(t => t.ReportedAt)
            .Select(t => new TicketResponseDto(
                t.Id,
                t.IssueTitle,
                t.Description,
                t.EmergencyLevel,
                t.Status,
                t.ReportedAt,
                t.WorkspaceId,
                t.Workspace.Name,
                t.UserId,
                t.User.FullName,
                t.TicketUpdates
                    .OrderBy(u => u.Timestamp)
                    .Select(u => new TicketUpdateResponseDto(
                        u.Id,
                        u.Message,
                        u.Timestamp,
                        u.IsAdminReply,
                        u.UserId,
                        u.User.FullName
                    ))
            ))
            .ToListAsync();
    }

    public async Task<TicketResponseDto?> GetMaintenanceTicketByIdAsync(int ticketId, string? userId = null)
    {
        var query = _context.MaintenanceTickets
            .AsNoTracking()
            .Where(t => t.Id == ticketId);

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(t => t.UserId == userId);
        }

        return await query
            .Select(t => new TicketResponseDto(
                t.Id,
                t.IssueTitle,
                t.Description,
                t.EmergencyLevel,
                t.Status,
                t.ReportedAt,
                t.WorkspaceId,
                t.Workspace.Name,
                t.UserId,
                t.User.FullName,
                t.TicketUpdates
                    .OrderBy(u => u.Timestamp)
                    .Select(u => new TicketUpdateResponseDto(
                        u.Id,
                        u.Message,
                        u.Timestamp,
                        u.IsAdminReply,
                        u.UserId,
                        u.User.FullName
                    ))
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TicketResponseDto>> GetUserMaintenanceTicketsAsync(
        string userId,
        TicketStatus? status = null,
        TicketEmergencyLevel? emergencyLevel = null)
    {
        var query = _context.MaintenanceTickets
            .AsNoTracking()
            .Where(t => t.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (emergencyLevel.HasValue)
        {
            query = query.Where(t => t.EmergencyLevel == emergencyLevel.Value);
        }

        return await query
            .OrderByDescending(t => t.ReportedAt)
            .Select(t => new TicketResponseDto(
                t.Id,
                t.IssueTitle,
                t.Description,
                t.EmergencyLevel,
                t.Status,
                t.ReportedAt,
                t.WorkspaceId,
                t.Workspace.Name,
                t.UserId,
                t.User.FullName,
                t.TicketUpdates
                    .OrderBy(u => u.Timestamp)
                    .Select(u => new TicketUpdateResponseDto(
                        u.Id,
                        u.Message,
                        u.Timestamp,
                        u.IsAdminReply,
                        u.UserId,
                        u.User.FullName
                    ))
            ))
            .ToListAsync();
    }

    public async Task<bool> UpdateMaintenanceTicketStatusAsync(
        int ticketId,
        UpdateTicketStatusDto dto,
        string? userId = null)
    {
        var query = _context.MaintenanceTickets.Where(t => t.Id == ticketId);

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(t => t.UserId == userId);
        }

        var ticket = await query.FirstOrDefaultAsync();
        if (ticket == null)
        {
            return false;
        }

        if (ticket.Status == dto.Status)
        {
            throw new InvalidOperationException("The ticket is already in the specified status.");
        }

        ticket.Status = dto.Status;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TicketUpdateResponseDto> AddTicketUpdateAsync(
        int ticketId,
        string userId,
        bool isAdmin,
        AddTicketUpdateDto dto)
    {
        var query = _context.MaintenanceTickets.Where(t => t.Id == ticketId);

        if (!isAdmin)
        {
            query = query.Where(t => t.UserId == userId);
        }

        var ticket = await query.FirstOrDefaultAsync();

        if (ticket == null)
        {
            throw new InvalidOperationException("Ticket not found.");
        }

        if (ticket.Status == TicketStatus.Closed)
        {
            throw new InvalidOperationException("Cannot add updates to a closed maintenance ticket.");
        }

        var update = new TicketUpdate
        {
            Message = dto.Message,
            Timestamp = DateTime.UtcNow,
            IsAdminReply = isAdmin,
            UserId = userId,
            MaintenanceTicketId = ticketId
        };

        _context.TicketUpdates.Add(update);
        await _context.SaveChangesAsync();
        await _context.Entry(update).Reference(u => u.User).LoadAsync();

        return new TicketUpdateResponseDto(
            update.Id,
            update.Message,
            update.Timestamp,
            update.IsAdminReply,
            update.UserId,
            update.User?.FullName ?? update.User?.UserName ?? "User"
        );
    }

    public async Task<bool> DeleteMaintenanceTicketAsync(int ticketId, string? userId = null)
    {
        var query = _context.MaintenanceTickets.Where(t => t.Id == ticketId);

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(t => t.UserId == userId);
        }

        var ticket = await query.FirstOrDefaultAsync();
        if (ticket == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(userId) && ticket.Status != TicketStatus.Open)
        {
            throw new InvalidOperationException("You can only delete tickets that are currently in Open status.");
        }

        _context.MaintenanceTickets.Remove(ticket);
        var result = await _context.SaveChangesAsync();

        return result > 0;
    }

    public async Task<bool> DeleteTicketUpdateAsync(int ticketId, int updateId)
    {
        var update = await _context.TicketUpdates
            .FirstOrDefaultAsync(u => u.Id == updateId && u.MaintenanceTicketId == ticketId);

        if (update == null)
        {
            return false;
        }

        _context.TicketUpdates.Remove(update);
        var result = await _context.SaveChangesAsync();

        return result > 0;
    }
}

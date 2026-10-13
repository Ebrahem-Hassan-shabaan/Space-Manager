using SpaceManager.DTOs.Workspaces;
using SpaceManager.Services.Interfaces;
using SpaceManager.Data;
using Microsoft.EntityFrameworkCore;
using SpaceManager.Models;

namespace SpaceManager.Services.Implementations;

/// <summary>
/// Provides business logic implementation for workspace operations.
/// </summary>
public class WorkspaceService : IWorkspaceService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkspaceService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public WorkspaceService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<WorkspaceResponseDto> CreateAsync(CreateWorkspaceDto dto)
    {
        var workspace = new Workspace
        {
            Name = dto.Name,
            Capacity = dto.Capacity,
            PricePerHour = dto.PricePerHour,
            WorkspaceTypeId = dto.WorkspaceTypeId
        };

        _context.Workspaces.Add(workspace);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(workspace.Id))!;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        var workspace = await _context.Workspaces.FindAsync(id);
        if (workspace == null)
        {
            return false;
        }

        var hasBookings = await _context.WorkspaceBookings.AnyAsync(b => b.WorkspaceId == id);
        var hasTickets = await _context.MaintenanceTickets.AnyAsync(t => t.WorkspaceId == id);
        if (hasBookings || hasTickets)
        {
            throw new InvalidOperationException("Cannot delete workspace with existing bookings or maintenance tickets.");
        }

        _context.Workspaces.Remove(workspace);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WorkspaceResponseDto>> GetAllAsync(int? workspaceTypeId = null, bool? isAvailable = null, int? maxCapacity = null, decimal? maxPricePerHour = null)
    {
        var query = _context.Workspaces.AsNoTracking().AsQueryable();

        if (workspaceTypeId != null)
        {
            query = query.Where(w => w.WorkspaceTypeId == workspaceTypeId);
        }

        if (isAvailable != null)
        {
            query = query.Where(w => w.IsAvailable == isAvailable);
        }

        if (maxCapacity != null)
        {
            query = query.Where(w => w.Capacity <= maxCapacity);
        }

        if (maxPricePerHour != null)
        {
            query = query.Where(w => w.PricePerHour <= maxPricePerHour);
        }

        return await query
            .Select(w => new WorkspaceResponseDto(
                w.Id,
                w.Name,
                w.PricePerHour,
                w.Capacity,
                w.IsAvailable,
                w.WorkspaceTypeId,
                w.WorkspaceType.Name
            ))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<WorkspaceResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Workspaces
            .AsNoTracking()
            .Where(w => w.Id == id)
            .Select(w => new WorkspaceResponseDto(
                w.Id,
                w.Name,
                w.PricePerHour,
                w.Capacity,
                w.IsAvailable,
                w.WorkspaceTypeId,
                w.WorkspaceType.Name
            ))
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<WorkspaceResponseDto?> UpdateAsync(int id, UpdateWorkspaceDto dto)
    {
        var workspace = await _context.Workspaces.FindAsync(id);
        if (workspace == null)
        {
            return null;
        }

        workspace.Name = dto.Name;
        workspace.PricePerHour = dto.PricePerHour;
        workspace.Capacity = dto.Capacity;
        workspace.WorkspaceTypeId = dto.WorkspaceTypeId;
        workspace.IsAvailable = dto.IsAvailable;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(workspace.Id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WorkspaceTypeResponseDto>> GetWorkspaceTypesAsync()
    {
        return await _context.WorkspaceTypes
            .AsNoTracking()
            .Select(wt => new WorkspaceTypeResponseDto(wt.Id, wt.Name))
            .ToListAsync();
    }
}

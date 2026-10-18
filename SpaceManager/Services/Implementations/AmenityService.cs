using Microsoft.EntityFrameworkCore;
using SpaceManager.Data;
using SpaceManager.DTOs.Amenities;
using SpaceManager.Models;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Services.Implementations;

/// <summary>
/// Provides business logic implementation for managing amenities and workspace assignments.
/// </summary>
public class AmenityService : IAmenityService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AmenityService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public AmenityService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AmenityResponseDto>> GetAllAsync()
    {
        return await _context.Amenities
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new AmenityResponseDto(a.Id, a.Name, a.Description))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<AmenityResponseDto?> GetByIdAsync(int id)
    {
        var amenity = await _context.Amenities.FindAsync(id);
        if (amenity == null)
        {
            return null;
        }

        return new AmenityResponseDto(amenity.Id, amenity.Name, amenity.Description);
    }

    /// <inheritdoc />
    public async Task<AmenityResponseDto> CreateAsync(CreateAmenityDto dto)
    {
        var amenity = new Amenity
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim()
        };

        _context.Amenities.Add(amenity);
        await _context.SaveChangesAsync();

        return new AmenityResponseDto(amenity.Id, amenity.Name, amenity.Description);
    }

    /// <inheritdoc />
    public async Task<AmenityResponseDto?> UpdateAsync(int id, UpdateAmenityDto dto)
    {
        var amenity = await _context.Amenities.FindAsync(id);
        if (amenity == null)
        {
            return null;
        }

        amenity.Name = dto.Name.Trim();
        amenity.Description = dto.Description?.Trim();

        await _context.SaveChangesAsync();

        return new AmenityResponseDto(amenity.Id, amenity.Name, amenity.Description);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        var amenity = await _context.Amenities.FindAsync(id);
        if (amenity == null)
        {
            return false;
        }

        var isAssigned = await _context.WorkspaceAmenities.AnyAsync(wa => wa.AmenityId == id);
        if (isAssigned)
        {
            throw new InvalidOperationException($"Cannot delete amenity '{amenity.Name}' because it is assigned to one or more workspaces. Remove all assignments first.");
        }

        _context.Amenities.Remove(amenity);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WorkspaceAmenityResponseDto>> GetByWorkspaceIdAsync(int workspaceId)
    {
        return await _context.WorkspaceAmenities
            .AsNoTracking()
            .Where(wa => wa.WorkspaceId == workspaceId)
            .Include(wa => wa.Amenity)
            .Select(wa => new WorkspaceAmenityResponseDto(
                wa.AmenityId,
                wa.Amenity.Name,
                wa.Amenity.Description,
                wa.IsOperational))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> AssignToWorkspaceAsync(int workspaceId, AssignWorkspaceAmenityDto dto)
    {
        var workspaceExists = await _context.Workspaces.AnyAsync(w => w.Id == workspaceId);
        if (!workspaceExists)
        {
            throw new ArgumentException($"Workspace with ID {workspaceId} was not found.");
        }

        var amenityExists = await _context.Amenities.AnyAsync(a => a.Id == dto.AmenityId);
        if (!amenityExists)
        {
            throw new ArgumentException($"Amenity with ID {dto.AmenityId} was not found.");
        }

        var existingAssignment = await _context.WorkspaceAmenities
            .FindAsync(workspaceId, dto.AmenityId);

        if (existingAssignment != null)
        {
            // Idempotent: if already assigned, update its operational status
            existingAssignment.IsOperational = dto.IsOperational;
        }
        else
        {
            var workspaceAmenity = new WorkspaceAmenity
            {
                WorkspaceId = workspaceId,
                AmenityId = dto.AmenityId,
                IsOperational = dto.IsOperational
            };
            _context.WorkspaceAmenities.Add(workspaceAmenity);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveFromWorkspaceAsync(int workspaceId, int amenityId)
    {
        var assignment = await _context.WorkspaceAmenities.FindAsync(workspaceId, amenityId);
        if (assignment == null)
        {
            return false;
        }

        _context.WorkspaceAmenities.Remove(assignment);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> SetOperationalStatusAsync(int workspaceId, int amenityId, bool isOperational)
    {
        var assignment = await _context.WorkspaceAmenities.FindAsync(workspaceId, amenityId);
        if (assignment == null)
        {
            return false;
        }

        assignment.IsOperational = isOperational;
        await _context.SaveChangesAsync();
        return true;
    }
}

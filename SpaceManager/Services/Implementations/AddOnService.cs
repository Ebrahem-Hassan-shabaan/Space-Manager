using Microsoft.EntityFrameworkCore;
using SpaceManager.Data;
using SpaceManager.DTOs.ServiceAddOns;
using SpaceManager.Models;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Services.Implementations;

/// <summary>
/// Provides business logic implementation for managing service add-ons and calculating their costs.
/// </summary>
public class AddOnService : IAddOnService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddOnService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public AddOnService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ServiceAddOnResponseDto>> GetAllAsync(bool? onlyAvailable = null)
    {
        var query = _context.ServiceAddOns.AsNoTracking().AsQueryable();

        if (onlyAvailable.HasValue && onlyAvailable.Value)
        {
            query = query.Where(s => s.IsAvailable);
        }

        return await query
            .OrderBy(s => s.Name)
            .Select(s => new ServiceAddOnResponseDto(s.Id, s.Name, s.Price, s.IsAvailable))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<ServiceAddOnResponseDto?> GetByIdAsync(int id)
    {
        var service = await _context.ServiceAddOns.FindAsync(id);
        if (service == null)
        {
            return null;
        }

        return new ServiceAddOnResponseDto(service.Id, service.Name, service.Price, service.IsAvailable);
    }

    /// <inheritdoc />
    public async Task<ServiceAddOnResponseDto> CreateAsync(CreateServiceAddOnDto dto)
    {
        var service = new ServiceAddOn
        {
            Name = dto.Name.Trim(),
            Price = dto.Price,
            IsAvailable = dto.IsAvailable
        };

        _context.ServiceAddOns.Add(service);
        await _context.SaveChangesAsync();

        return new ServiceAddOnResponseDto(service.Id, service.Name, service.Price, service.IsAvailable);
    }

    /// <inheritdoc />
    public async Task<ServiceAddOnResponseDto?> UpdateAsync(int id, UpdateServiceAddOnDto dto)
    {
        var service = await _context.ServiceAddOns.FindAsync(id);
        if (service == null)
        {
            return null;
        }

        service.Name = dto.Name.Trim();
        service.Price = dto.Price;
        service.IsAvailable = dto.IsAvailable;

        await _context.SaveChangesAsync();

        return new ServiceAddOnResponseDto(service.Id, service.Name, service.Price, service.IsAvailable);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        var service = await _context.ServiceAddOns.FindAsync(id);
        if (service == null)
        {
            return false;
        }

        var hasBookings = await _context.BookingAddOns.AnyAsync(ba => ba.ServiceAddOnId == id);
        if (hasBookings)
        {
            throw new InvalidOperationException(
                $"Cannot delete service add-on '{service.Name}' because it is associated with existing bookings. " +
                "Consider deactivating it (setting IsAvailable = false) to retain financial and booking history.");
        }

        _context.ServiceAddOns.Remove(service);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BookingAddOnResponseDto>> GetByBookingIdAsync(int bookingId)
    {
        return await _context.BookingAddOns
            .AsNoTracking()
            .Where(ba => ba.WorkspaceBookingId == bookingId)
            .Include(ba => ba.ServiceAddOn)
            .Select(ba => new BookingAddOnResponseDto(
                ba.ServiceAddOnId,
                ba.ServiceAddOn.Name,
                ba.Quantity > 0 ? (ba.TotalPrice / ba.Quantity) : ba.ServiceAddOn.Price,
                ba.Quantity,
                ba.TotalPrice))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<decimal> CalculateTotalCostAsync(IEnumerable<BookingAddOnRequestDto> addOns)
    {
        if (addOns == null)
        {
            return 0m;
        }

        var requestList = addOns.ToList();
        if (!requestList.Any())
        {
            return 0m;
        }

        var requestedIds = requestList.Select(a => a.ServiceAddOnId).Distinct().ToList();

        // Batch query all requested services in a single round-trip
        var services = await _context.ServiceAddOns
            .Where(s => requestedIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id);

        decimal totalCost = 0m;

        foreach (var item in requestList)
        {
            if (!services.TryGetValue(item.ServiceAddOnId, out var service))
            {
                throw new InvalidOperationException($"Service add-on with ID {item.ServiceAddOnId} was not found.");
            }

            if (!service.IsAvailable)
            {
                throw new InvalidOperationException($"Service add-on '{service.Name}' is currently marked unavailable.");
            }

            if (item.Quantity <= 0)
            {
                throw new ArgumentException($"Quantity for service add-on '{service.Name}' must be greater than zero.");
            }

            totalCost += service.Price * item.Quantity;
        }

        return totalCost;
    }
}

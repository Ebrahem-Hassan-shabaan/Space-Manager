using SpaceManager.DTOs.ServiceAddOns;

namespace SpaceManager.Services.Interfaces;

/// <summary>
/// Defines service operations for managing bookable service add-ons and calculating their costs.
/// </summary>
public interface IAddOnService
{
    /// <summary>
    /// Retrieves all service add-ons in the catalog, optionally filtering by availability.
    /// </summary>
    /// <param name="onlyAvailable">Optional filter to return only currently available add-ons.</param>
    /// <returns>A collection of service add-on response DTOs.</returns>
    Task<IEnumerable<ServiceAddOnResponseDto>> GetAllAsync(bool? onlyAvailable = null);

    /// <summary>
    /// Retrieves a service add-on by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the service add-on.</param>
    /// <returns>The service add-on response DTO if found; otherwise, null.</returns>
    Task<ServiceAddOnResponseDto?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new service add-on in the catalog.
    /// </summary>
    /// <param name="dto">The creation request data.</param>
    /// <returns>The created service add-on response DTO.</returns>
    Task<ServiceAddOnResponseDto> CreateAsync(CreateServiceAddOnDto dto);

    /// <summary>
    /// Updates an existing service add-on by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the service add-on to update.</param>
    /// <param name="dto">The update request data.</param>
    /// <returns>The updated service add-on response DTO if found; otherwise, null.</returns>
    Task<ServiceAddOnResponseDto?> UpdateAsync(int id, UpdateServiceAddOnDto dto);

    /// <summary>
    /// Deletes a service add-on by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the service add-on to delete.</param>
    /// <returns>True if the service add-on was deleted; false if not found.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the service add-on is associated with existing bookings.</exception>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Retrieves all add-ons included in a specific workspace booking.
    /// </summary>
    /// <param name="bookingId">The unique identifier of the workspace booking.</param>
    /// <returns>A collection of booking add-on response DTOs.</returns>
    Task<IEnumerable<BookingAddOnResponseDto>> GetByBookingIdAsync(int bookingId);

    /// <summary>
    /// Calculates the total cost for a collection of requested add-ons based on current prices.
    /// </summary>
    /// <param name="addOns">The collection of requested add-ons with quantities.</param>
    /// <returns>The total calculated cost of the add-on items.</returns>
    /// <exception cref="InvalidOperationException">Thrown if any requested add-on does not exist or is unavailable.</exception>
    Task<decimal> CalculateTotalCostAsync(IEnumerable<BookingAddOnRequestDto>? addOns);
}

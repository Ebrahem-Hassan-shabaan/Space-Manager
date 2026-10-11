using SpaceManager.DTOs.Bookings;

namespace SpaceManager.Services.Interfaces;

/// <summary>
/// Defines service operations for managing workspace reservations.
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Creates a new workspace reservation for the specified user.
    /// Validates availability, checks for scheduling conflicts,
    /// and calculates the total cost based on duration and hourly rate.
    /// </summary>
    /// <param name="userId">The unique identifier of the authenticated user making the reservation.</param>
    /// <param name="dto">The booking creation request containing WorkspaceId, StartTime, and EndTime.</param>
    /// <returns>The confirmed booking response DTO with the calculated total cost.</returns>
    Task<BookingResponseDto> CreateBookingAsync(string userId, CreateBookingDto dto);

    /// <summary>
    /// Retrieves bookings for the specified user with optional filtering.
    /// </summary>
    /// <param name="userId">The unique identifier of the authenticated user.</param>
    /// <param name="upcoming">
    /// Optional filter:
    /// <c>true</c> returns upcoming bookings only (StartTime > UtcNow);
    /// <c>false</c> returns past and current (ongoing) bookings (StartTime &lt;= UtcNow);
    /// <c>null</c> returns all bookings.
    /// </param>
    /// <returns>A collection of booking response DTOs matching the specified filter.</returns>
    Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(string userId, bool? upcoming = null);

    /// <summary>
    /// Cancels an existing reservation.
    /// If userId is specified, ensures the reservation belongs to that user (ownership check).
    /// If userId is null, allows cancellation regardless of owner (admin mode).
    /// Cancellation is only permitted if the reservation start time is at least 1 hour in the future.
    /// </summary>
    /// <param name="bookingId">The unique identifier of the booking to cancel.</param>
    /// <param name="userId">Optional unique identifier of the user for ownership validation.</param>
    /// <returns>
    /// <c>true</c> if the booking was successfully cancelled;
    /// <c>false</c> if the booking was not found or does not belong to the user.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the reservation's start time has passed or starts within 1 hour.
    /// </exception>
    Task<bool> CancelBookingAsync(int bookingId, string? userId = null);

    /// <summary>
    /// Retrieves all bookings across all users. (Admin use only)
    /// </summary>
    /// <param name="upcoming">
    /// Optional filter:
    /// <c>true</c> returns upcoming bookings only (StartTime > UtcNow);
    /// <c>false</c> returns past and current (ongoing) bookings (StartTime &lt;= UtcNow);
    /// <c>null</c> returns all bookings.
    /// </param>
    /// <returns>A collection of all booking response DTOs matching the specified filter.</returns>
    Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync(bool? upcoming = null);

    /// <summary>
    /// Retrieves a single booking by its unique identifier.
    /// If userId is specified, only returns the booking if it belongs to that user (ownership check).
    /// If userId is null, returns the booking regardless of owner (admin mode).
    /// </summary>
    /// <param name="bookingId">The unique identifier of the booking.</param>
    /// <param name="userId">Optional user identifier for ownership validation.</param>
    /// <returns>The booking response DTO if found; otherwise, null.</returns>
    Task<BookingResponseDto?> GetBookingByIdAsync(int bookingId, string? userId = null);
}

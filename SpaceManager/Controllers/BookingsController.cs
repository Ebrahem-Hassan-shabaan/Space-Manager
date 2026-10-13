using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceManager.DTOs.Bookings;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Controllers;

/// <summary>
/// Provides API endpoints for managing workspace reservations and bookings.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookingsController"/> class.
    /// </summary>
    /// <param name="bookingService">The booking service instance.</param>
    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Retrieves bookings with optional status filtering.
    /// Admin users retrieve bookings across all users. Regular members retrieve their own bookings only.
    /// </summary>
    /// <param name="upcoming">
    /// Optional filter:
    /// <c>true</c> returns upcoming bookings only (StartTime > UtcNow);
    /// <c>false</c> returns past and current (ongoing) bookings (StartTime &lt;= UtcNow);
    /// <c>null</c> returns all bookings.
    /// </param>
    /// <returns>A collection of matching booking response DTOs.</returns>
    /// <response code="200">Returns the matching list of bookings.</response>
    /// <response code="401">If the user is unauthenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BookingResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBookings([FromQuery] bool? upcoming = null)
    {
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (userRole == null)
        {
            return Unauthorized();
        }

        if (userRole == "Admin")
        {
            var allBookings = await _bookingService.GetAllBookingsAsync(upcoming);
            return Ok(allBookings);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var userBookings = await _bookingService.GetUserBookingsAsync(userId, upcoming);
        return Ok(userBookings);
    }

    /// <summary>
    /// Retrieves details of a specific booking by its ID.
    /// Admins can access any booking; regular members can only retrieve their own.
    /// </summary>
    /// <param name="id">The unique identifier of the booking.</param>
    /// <returns>The booking response DTO if found.</returns>
    /// <response code="200">Returns the booking details.</response>
    /// <response code="401">If the user is unauthenticated.</response>
    /// <response code="404">If the booking does not exist or does not belong to the caller.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookingById(int id)
    {
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        // Admin can view any booking; normal user can only view their own
        var booking = await _bookingService.GetBookingByIdAsync(id, userRole == "Admin" ? null : userId);
        if (booking == null)
        {
            return NotFound(new { message = "Booking not found." });
        }

        return Ok(booking);
    }

    /// <summary>
    /// Creates a new workspace reservation.
    /// Validates workspace availability, ensures no schedule conflicts, and calculates total cost.
    /// </summary>
    /// <param name="dto">The booking creation request payload.</param>
    /// <returns>The confirmed booking response DTO.</returns>
    /// <response code="201">Returns the created booking and sets the Location header.</response>
    /// <response code="400">If the input dates are invalid or a scheduling conflict exists.</response>
    /// <response code="401">If the user is unauthenticated.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var booking = await _bookingService.CreateBookingAsync(userId, dto);
            return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id }, booking);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancels an existing workspace reservation.
    /// Cancellation is only permitted at least 1 hour before the reservation start time.
    /// Admins can cancel any booking; regular members can only cancel their own.
    /// </summary>
    /// <param name="id">The unique identifier of the booking to cancel.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the booking was successfully cancelled.</response>
    /// <response code="400">If the booking is already cancelled or cannot be cancelled within 1 hour of start time.</response>
    /// <response code="401">If the user is unauthenticated.</response>
    /// <response code="404">If the booking is not found or does not belong to the caller.</response>
    [HttpPatch("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelBooking(int id)
    {
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var success = await _bookingService.CancelBookingAsync(id, userRole == "Admin" ? null : userId);
            if (!success)
            {
                return NotFound(new { message = "Booking not found." });
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

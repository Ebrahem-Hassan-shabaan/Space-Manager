using SpaceManager.DTOs.ServiceAddOns;
using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.Bookings;

/// <summary>
/// Data transfer object used to create a new workspace reservation.
/// </summary>
/// <param name="WorkspaceId">The unique identifier of the workspace to reserve.</param>
/// <param name="StartTime">The start date and time of the reservation.</param>
/// <param name="EndTime">The end date and time of the reservation.</param>
/// <param name="BookingAddons">Optional collection of service add-ons to attach to the reservation.</param>
public record CreateBookingDto(
    [Required] int WorkspaceId,
    [Required] DateTime StartTime,
    [Required] DateTime EndTime,
    List<BookingAddOnRequestDto>? BookingAddons = null
);

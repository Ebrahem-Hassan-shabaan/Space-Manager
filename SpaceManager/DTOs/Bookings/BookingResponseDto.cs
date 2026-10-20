using SpaceManager.Enums;
using SpaceManager.DTOs.ServiceAddOns;
using System.Collections.Generic;

namespace SpaceManager.DTOs.Bookings;

/// <summary>
/// Data transfer object representing a workspace reservation response.
/// </summary>
/// <param name="Id">The unique identifier of the booking.</param>
/// <param name="WorkspaceId">The unique identifier of the reserved workspace.</param>
/// <param name="WorkspaceName">The name of the reserved workspace.</param>
/// <param name="StartTime">The start date and time of the reservation.</param>
/// <param name="EndTime">The end date and time of the reservation.</param>
/// <param name="TotalCost">The calculated total cost of the reservation.</param>
/// <param name="Status">The current status of the reservation.</param>
/// <param name="BookingAddOns">The optional list of service add-ons included in the booking.</param>
public record BookingResponseDto(
    int Id,
    int WorkspaceId,
    string WorkspaceName,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalCost,
    BookingStatus Status,
    List<BookingAddOnResponseDto>? BookingAddOns = null
);

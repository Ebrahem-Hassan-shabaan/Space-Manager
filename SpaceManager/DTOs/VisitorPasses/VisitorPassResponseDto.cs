namespace SpaceManager.DTOs.VisitorPasses;

/// <summary>
/// Data transfer object representing a visitor pass response.
/// </summary>
/// <param name="Id">The unique identifier of the visitor pass.</param>
/// <param name="WorkspaceBookingId">The associated booking identifier.</param>
/// <param name="WorkspaceName">The name of the reserved workspace.</param>
/// <param name="HostUserId">The ID of the member hosting the visitor.</param>
/// <param name="HostName">The full name of the member hosting the visitor.</param>
/// <param name="GuestName">The full name of the visiting guest.</param>
/// <param name="GuestEmail">The email address of the visiting guest.</param>
/// <param name="ExpectedArrival">The scheduled arrival date and time.</param>
/// <param name="ActualCheckInTime">The timestamp when the guest checked in, or null if pending.</param>
/// <param name="IsCheckedIn">Indicates whether the guest has checked in.</param>
/// <param name="BookingEndTime">The end time of the associated workspace booking, if available.</param>
/// <param name="BookingStartTime">The start time of the associated workspace booking, if available.</param>
public record VisitorPassResponseDto(
    int Id,
    int WorkspaceBookingId,
    string WorkspaceName,
    string HostUserId,
    string HostName,
    string GuestName,
    string GuestEmail,
    DateTime ExpectedArrival,
    DateTime? ActualCheckInTime,
    bool IsCheckedIn,
    DateTime? BookingEndTime = null,
    DateTime? BookingStartTime = null
);

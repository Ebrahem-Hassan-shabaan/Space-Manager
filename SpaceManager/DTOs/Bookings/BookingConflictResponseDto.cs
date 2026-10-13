namespace SpaceManager.DTOs.Bookings;

/// <summary>
/// Represents an existing reserved time interval that conflicts with a requested reservation.
/// Does not expose any sensitive internal booking or user IDs.
/// </summary>
/// <param name="StartTime">The start date and time (UTC) of the conflicting reservation.</param>
/// <param name="EndTime">The end date and time (UTC) of the conflicting reservation.</param>
public record ConflictingSlotDto(
    DateTime StartTime,
    DateTime EndTime
);

/// <summary>
/// Structured response returned when a workspace reservation fails due to schedule overlap.
/// </summary>
/// <param name="Message">User-friendly error message.</param>
/// <param name="WorkspaceId">The workspace identifier requested.</param>
/// <param name="RequestedStartTime">The requested start time (UTC).</param>
/// <param name="RequestedEndTime">The requested end time (UTC).</param>
/// <param name="ConflictingSlots">Collection of time slots currently occupied.</param>
public record BookingConflictResponseDto(
    string Message,
    int WorkspaceId,
    DateTime RequestedStartTime,
    DateTime RequestedEndTime,
    IReadOnlyList<ConflictingSlotDto> ConflictingSlots
);

/// <summary>
/// Domain exception thrown when a requested booking overlaps with existing confirmed reservations.
/// </summary>
public class BookingConflictException : InvalidOperationException
{
    public BookingConflictResponseDto ConflictDetails { get; }

    public BookingConflictException(BookingConflictResponseDto conflictDetails)
        : base(conflictDetails.Message)
    {
        ConflictDetails = conflictDetails;
    }
}

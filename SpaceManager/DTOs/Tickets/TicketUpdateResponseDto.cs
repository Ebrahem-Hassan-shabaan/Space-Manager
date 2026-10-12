namespace SpaceManager.DTOs.Tickets;

/// <summary>
/// Data transfer object representing a comment or status update on a maintenance ticket.
/// </summary>
/// <param name="Id">The unique identifier of the update.</param>
/// <param name="Message">The update message or comment content.</param>
/// <param name="Timestamp">The timestamp when the update was posted.</param>
/// <param name="IsAdminReply">Indicates whether the update was posted by an administrator or staff member.</param>
/// <param name="UserId">The ID of the user who posted the update.</param>
/// <param name="UserName">The display name of the user who posted the update.</param>
public record TicketUpdateResponseDto(
    int Id,
    string Message,
    DateTime Timestamp,
    bool IsAdminReply,
    string UserId,
    string UserName
);

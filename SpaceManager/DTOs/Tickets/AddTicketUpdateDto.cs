using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.Tickets;

/// <summary>
/// Data transfer object for adding a comment or progress note to an existing ticket.
/// </summary>
/// <param name="Message">The update message or comment content.</param>
public record AddTicketUpdateDto(
    [Required]
    [MaxLength(1000)]
    string Message
);

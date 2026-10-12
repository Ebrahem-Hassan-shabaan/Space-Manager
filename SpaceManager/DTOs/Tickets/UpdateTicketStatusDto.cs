using System.ComponentModel.DataAnnotations;
using SpaceManager.Enums;

namespace SpaceManager.DTOs.Tickets;

/// <summary>
/// Data transfer object for updating the status of a maintenance ticket (Admin only).
/// </summary>
/// <param name="Status">The new status to assign to the ticket.</param>
public record UpdateTicketStatusDto(
    [Required]
    TicketStatus Status
);

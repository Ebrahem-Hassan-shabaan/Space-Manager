using System.ComponentModel.DataAnnotations;
using SpaceManager.Enums;

namespace SpaceManager.DTOs.Tickets;

/// <summary>
/// Data transfer object used to report a new maintenance issue.
/// </summary>
/// <param name="WorkspaceId">The unique identifier of the workspace needing maintenance.</param>
/// <param name="IssueTitle">A concise title summarizing the issue.</param>
/// <param name="Description">Detailed description of the problem.</param>
/// <param name="EmergencyLevel">The severity/urgency level of the issue.</param>
public record CreateTicketDto(
    [Required]
    int WorkspaceId,

    [Required]
    [MaxLength(150)]
    string IssueTitle,

    [Required]
    [MaxLength(1000)]
    string Description,

    [Required]
    TicketEmergencyLevel EmergencyLevel = TicketEmergencyLevel.Medium
);

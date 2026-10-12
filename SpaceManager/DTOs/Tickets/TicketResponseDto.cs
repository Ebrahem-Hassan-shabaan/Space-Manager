using SpaceManager.Enums;

namespace SpaceManager.DTOs.Tickets;

/// <summary>
/// Data transfer object representing a maintenance ticket.
/// </summary>
/// <param name="Id">The unique identifier of the maintenance ticket.</param>
/// <param name="IssueTitle">The title of the reported issue.</param>
/// <param name="Description">The detailed description of the problem.</param>
/// <param name="EmergencyLevel">The severity level (Low, Medium, High, Critical).</param>
/// <param name="Status">The current status of the ticket (Open, InProgress, Resolved, Closed).</param>
/// <param name="ReportedAt">The timestamp when the ticket was reported.</param>
/// <param name="WorkspaceId">The ID of the affected workspace.</param>
/// <param name="WorkspaceName">The name of the affected workspace.</param>
/// <param name="ReportedByUserId">The ID of the user who reported the ticket.</param>
/// <param name="ReportedByUserName">The full name of the user who reported the ticket.</param>
/// <param name="Updates">The collection of updates and comments associated with this ticket.</param>
public record TicketResponseDto(
    int Id,
    string IssueTitle,
    string Description,
    TicketEmergencyLevel EmergencyLevel,
    TicketStatus Status,
    DateTime ReportedAt,
    int WorkspaceId,
    string WorkspaceName,
    string ReportedByUserId,
    string ReportedByUserName,
    IEnumerable<TicketUpdateResponseDto> Updates
);

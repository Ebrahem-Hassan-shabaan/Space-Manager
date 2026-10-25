namespace SpaceManager.Enums;

/// <summary>
/// Represents the resolution workflow stage of a maintenance ticket.
/// </summary>
public enum TicketStatus
{
    /// <summary>
    /// The ticket has been submitted and is awaiting staff review.
    /// </summary>
    Open = 1,

    /// <summary>
    /// Maintenance personnel are actively investigating or remediating the issue.
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// The physical defect has been fixed and awaits confirmation.
    /// </summary>
    Resolved = 3,

    /// <summary>
    /// The ticket workflow has concluded and the case is closed.
    /// </summary>
    Closed = 4
}

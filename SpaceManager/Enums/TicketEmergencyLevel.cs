namespace SpaceManager.Enums;

/// <summary>
/// Defines the urgency and operational impact of a facility maintenance ticket.
/// </summary>
public enum TicketEmergencyLevel
{
    /// <summary>
    /// Minor or cosmetic issue that does not impede workspace usage.
    /// </summary>
    Low = 1,

    /// <summary>
    /// Moderate issue requiring prompt attention during standard operating hours.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// Significant defect impacting member productivity or room functionality.
    /// </summary>
    High = 3,

    /// <summary>
    /// Severe hazard, power outage, or emergency requiring immediate response.
    /// </summary>
    Critical = 4
}

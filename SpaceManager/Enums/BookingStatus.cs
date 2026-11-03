namespace SpaceManager.Enums;

/// <summary>
/// Represents the lifecycle reservation status of a workspace booking.
/// </summary>
public enum BookingStatus
{
    /// <summary>
    /// The reservation is approved, active, and holding the space schedule.
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// The reservation was cancelled by the user or an administrator.
    /// </summary>
    Cancelled = 2,

    /// <summary>
    /// The scheduled end time of the reservation has passed and the session concluded.
    /// </summary>
    Completed = 3,
    PendingPayment = 4,
}

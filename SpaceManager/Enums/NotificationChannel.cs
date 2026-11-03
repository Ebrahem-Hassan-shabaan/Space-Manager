namespace SpaceManager.Enums;

/// <summary>
/// Specifies the delivery channel for dispatching notification messages.
/// Designed for Open/Closed Principle extension (InApp, Email, SMS, Mobile Push).
/// </summary>
public enum NotificationChannel
{
    /// <summary>
    /// Stored internally in the database; rendered in the notification bell and user dashboard.
    /// </summary>
    InApp = 1,

    /// <summary>
    /// Dispatched via email gateway (future extension).
    /// </summary>
    Email = 2,

    /// <summary>
    /// Dispatched via SMS text messaging provider (future extension).
    /// </summary>
    Sms = 3,

    /// <summary>
    /// Dispatched via mobile or web push notification service (future extension).
    /// </summary>
    Push = 4
}

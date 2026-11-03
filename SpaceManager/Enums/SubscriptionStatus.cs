namespace SpaceManager.Enums;

/// <summary>
/// Represents the lifecycle state of a user's recurring membership subscription.
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>
    /// Subscription is currently active and granting booking privileges.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Subscription reached its expiration date without renewal.
    /// </summary>
    Expired = 2,

    /// <summary>
    /// Subscription was cancelled by the user or an administrator (valid until end date).
    /// </summary>
    Cancelled = 3
}

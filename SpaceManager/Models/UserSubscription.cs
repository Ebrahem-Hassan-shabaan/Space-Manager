using SpaceManager.Enums;

namespace SpaceManager.Models;

/// <summary>
/// Represents an active or historical membership subscription held by an individual user.
/// </summary>
public class UserSubscription
{
    /// <summary>
    /// Gets or sets the primary key identifier of the user subscription.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the membership period began.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the membership period expires.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this subscription is currently valid and active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the subscription is set to automatically renew upon expiration.
    /// </summary>
    public bool IsAutoRenew { get; set; } = true;

    /// <summary>
    /// Gets or sets the lifecycle status of the subscription.
    /// </summary>
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    /// <summary>
    /// Gets or sets the foreign key of the user who holds this membership.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the navigation property to the member user.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the foreign key of the governing subscription plan tier.
    /// </summary>
    public int SubscriptionPlanId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the governing subscription plan.
    /// </summary>
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
}

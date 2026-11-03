namespace SpaceManager.Models;

/// <summary>
/// Defines a recurring membership tier offering access to specific workspace types or all-access platform privileges.
/// </summary>
public class SubscriptionPlan
{
    /// <summary>
    /// Gets or sets the primary key identifier of the subscription plan.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique commercial name of the membership tier (e.g., 'Hot Desk Unlimited', 'Private Office VIP').
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a description highlighting the amenities, perks, and inclusions of this tier.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recurring fee charged per billing cycle in Egyptian Pounds (EGP).
    /// </summary>
    public decimal MonthlyPrice { get; set; }

    /// <summary>
    /// Gets or sets the duration of each billing cycle in months (defaults to 1 month).
    /// </summary>
    public int DurationMonths { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum number of reservations allowed per calendar month (0 designates unlimited).
    /// </summary>
    public int MaxBookingsPerMonth { get; set; } = 0;

    /// <summary>
    /// Gets or sets the optional workspace type category covered by this plan.
    /// When null, this plan functions as an 'All-Access Pass' applicable to all workspace types.
    /// </summary>
    public int? WorkspaceTypeId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the covered workspace type classification.
    /// </summary>
    public WorkspaceType? WorkspaceType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the plan is publicly available for new subscriptions.
    /// Deactivating preserves historical subscriber records.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the collection of subscriber memberships associated with this plan.
    /// </summary>
    public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}

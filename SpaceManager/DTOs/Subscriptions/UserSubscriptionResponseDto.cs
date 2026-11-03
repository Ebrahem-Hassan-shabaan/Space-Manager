using SpaceManager.Enums;

namespace SpaceManager.DTOs.Subscriptions;

/// <summary>
/// Data transfer object detailing a user's active or historical membership subscription.
/// </summary>
public class UserSubscriptionResponseDto
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsAutoRenew { get; set; }
    public SubscriptionStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();

    // Plan details
    public int SubscriptionPlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public int DurationMonths { get; set; }
    public int MaxBookingsPerMonth { get; set; }
    public int? WorkspaceTypeId { get; set; }
    public string? WorkspaceTypeName { get; set; }
    public bool IsAllAccess => WorkspaceTypeId == null;

    // Remaining calculations
    public int DaysRemaining => Math.Max(0, (int)Math.Ceiling((EndDate - DateTime.UtcNow).TotalDays));
    public int BookingsUsedThisMonth { get; set; }
    public int? BookingsRemainingThisMonth => MaxBookingsPerMonth > 0 ? Math.Max(0, MaxBookingsPerMonth - BookingsUsedThisMonth) : null;
}

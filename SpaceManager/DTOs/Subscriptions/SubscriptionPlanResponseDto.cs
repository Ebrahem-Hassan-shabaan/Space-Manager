namespace SpaceManager.DTOs.Subscriptions;

/// <summary>
/// Data transfer object detailing a membership subscription plan.
/// </summary>
public class SubscriptionPlanResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public int DurationMonths { get; set; }
    public int MaxBookingsPerMonth { get; set; }
    public int? WorkspaceTypeId { get; set; }
    public string? WorkspaceTypeName { get; set; }
    public bool IsAllAccess => WorkspaceTypeId == null;
    public bool IsActive { get; set; }
    public int ActiveSubscribersCount { get; set; }
}

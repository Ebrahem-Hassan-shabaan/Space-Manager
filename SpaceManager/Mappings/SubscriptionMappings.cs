using SpaceManager.DTOs.Subscriptions;
using SpaceManager.Enums;
using SpaceManager.Models;

namespace SpaceManager.Mappings;

/// <summary>
/// Pure static mapping extensions projecting subscription plans and user memberships.
/// </summary>
public static class SubscriptionMappings
{
    public static SubscriptionPlan ToEntity(this CreateSubscriptionPlanDto dto) =>
        new()
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            MonthlyPrice = dto.MonthlyPrice,
            DurationMonths = dto.DurationMonths,
            MaxBookingsPerMonth = dto.MaxBookingsPerMonth,
            WorkspaceTypeId = dto.WorkspaceTypeId,
            IsActive = true
        };

    public static SubscriptionPlanResponseDto ToResponseDto(this SubscriptionPlan p) =>
        new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            MonthlyPrice = p.MonthlyPrice,
            DurationMonths = p.DurationMonths,
            MaxBookingsPerMonth = p.MaxBookingsPerMonth,
            WorkspaceTypeId = p.WorkspaceTypeId,
            WorkspaceTypeName = p.WorkspaceType?.Name,
            IsActive = p.IsActive,
            ActiveSubscribersCount = p.UserSubscriptions?.Count(s => s.IsActive && s.Status == SubscriptionStatus.Active) ?? 0
        };

    public static UserSubscriptionResponseDto ToResponseDto(this UserSubscription s, int bookingsUsedThisMonth = 0) =>
        new()
        {
            Id = s.Id,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            IsActive = s.IsActive,
            IsAutoRenew = s.IsAutoRenew,
            Status = s.Status,
            SubscriptionPlanId = s.SubscriptionPlanId,
            PlanName = s.SubscriptionPlan?.Name ?? string.Empty,
            MonthlyPrice = s.SubscriptionPlan?.MonthlyPrice ?? 0m,
            DurationMonths = s.SubscriptionPlan?.DurationMonths ?? 1,
            MaxBookingsPerMonth = s.SubscriptionPlan?.MaxBookingsPerMonth ?? 0,
            WorkspaceTypeId = s.SubscriptionPlan?.WorkspaceTypeId,
            WorkspaceTypeName = s.SubscriptionPlan?.WorkspaceType?.Name,
            BookingsUsedThisMonth = bookingsUsedThisMonth
        };
}

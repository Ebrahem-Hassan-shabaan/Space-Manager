using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceManager.Common;
using SpaceManager.DTOs.Subscriptions;

namespace SpaceManager.Services.Interfaces;

/// <summary>
/// Provides business operations for membership plans, subscription payments, automated renewals, and status lifecycle.
/// </summary>
public interface ISubscriptionService
{
    // Plan Catalog
    Task<ServiceResult<List<SubscriptionPlanResponseDto>>> GetAllActivePlansAsync();
    Task<ServiceResult<List<SubscriptionPlanResponseDto>>> GetAllPlansAdminAsync(CurrentUserContext caller);
    Task<ServiceResult<SubscriptionPlanResponseDto>> GetPlanByIdAsync(int id);
    Task<ServiceResult<SubscriptionPlanResponseDto>> CreatePlanAsync(CreateSubscriptionPlanDto dto, CurrentUserContext caller);
    Task<ServiceResult<SubscriptionPlanResponseDto>> UpdatePlanAsync(int id, UpdateSubscriptionPlanDto dto, CurrentUserContext caller);
    Task<ServiceResult> DeactivatePlanAsync(int id, CurrentUserContext caller);

    // Membership Subscriptions
    Task<ServiceResult<UserSubscriptionResponseDto>> SubscribeAsync(SubscribeRequestDto request, CurrentUserContext caller);
    Task<ServiceResult<UserSubscriptionResponseDto>> AdminAssignSubscriptionAsync(AdminAssignSubscriptionDto dto, CurrentUserContext caller);
    Task<ServiceResult<UserSubscriptionResponseDto?>> GetMySubscriptionAsync(CurrentUserContext caller);
    Task<ServiceResult<UserSubscriptionResponseDto?>> GetActiveSubscriptionForUserAsync(string userId);
    Task<ServiceResult> CancelSubscriptionAsync(CurrentUserContext caller);
    Task<ServiceResult> ToggleAutoRenewAsync(bool enable, CurrentUserContext caller);
}

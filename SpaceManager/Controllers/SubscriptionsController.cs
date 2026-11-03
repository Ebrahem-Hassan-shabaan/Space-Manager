using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceManager.Common;
using SpaceManager.DTOs.Subscriptions;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Controllers;

/// <summary>
/// Provides REST API endpoints for viewing membership tiers, self-service subscription purchasing,
/// renewal management, and administrative plan configuration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    /// <summary>
    /// Publicly retrieves all active membership subscription plans.
    /// </summary>
    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActivePlans()
    {
        var result = await _subscriptionService.GetAllActivePlansAsync();
        return Ok(result.Data);
    }

    /// <summary>
    /// Publicly retrieves details of a specific membership plan.
    /// </summary>
    [HttpGet("plans/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlanById(int id)
    {
        var result = await _subscriptionService.GetPlanByIdAsync(id);

        if (!result.Succeeded)
            return NotFound(new { message = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves the active membership subscription for the authenticated caller.
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMySubscription()
    {
        var caller = User.GetUserContext();
        var result = await _subscriptionService.GetMySubscriptionAsync(caller);

        if (!result.Succeeded)
            return result.IsForbidden ? Forbid() : BadRequest(new { message = result.Error });

        if (result.Data == null)
            return NotFound(new { message = "You do not currently have an active membership subscription." });

        return Ok(result.Data);
    }

    /// <summary>
    /// Subscribes the caller to a membership plan using the specified payment method.
    /// </summary>
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequestDto request)
    {
        var caller = User.GetUserContext();
        var result = await _subscriptionService.SubscribeAsync(request, caller);

        if (!result.Succeeded)
            return result.IsForbidden ? Forbid() : BadRequest(new { message = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Cancels the caller's active subscription. Privileges remain active until the billing cycle end date.
    /// </summary>
    [HttpDelete("cancel")]
    public async Task<IActionResult> CancelSubscription()
    {
        var caller = User.GetUserContext();
        var result = await _subscriptionService.CancelSubscriptionAsync(caller);

        if (!result.Succeeded)
            return result.IsForbidden ? Forbid() : BadRequest(new { message = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Toggles automatic renewal for the caller's active subscription.
    /// </summary>
    [HttpPatch("auto-renew")]
    public async Task<IActionResult> ToggleAutoRenew([FromQuery] bool enable)
    {
        var caller = User.GetUserContext();
        var result = await _subscriptionService.ToggleAutoRenewAsync(enable, caller);

        if (!result.Succeeded)
            return result.IsForbidden ? Forbid() : BadRequest(new { message = result.Error });

        return NoContent();
    }

    // --- Admin Endpoints ---

    /// <summary>
    /// Retrieves all subscription plans including archived/inactive tiers (Admin only).
    /// </summary>
    [HttpGet("admin/plans")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllPlansAdmin()
    {
        var caller = User.GetUserContext();
        var result = await _subscriptionService.GetAllPlansAdminAsync(caller);

        if (!result.Succeeded)
            return result.IsForbidden ? Forbid() : BadRequest(new { message = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Creates a new membership plan tier (Admin only).
    /// </summary>
    [HttpPost("plans")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePlan([FromBody] CreateSubscriptionPlanDto dto)
    {
        var caller = User.GetUserContext();
        var result = await _subscriptionService.CreatePlanAsync(dto, caller);

        if (!result.Succeeded)
            return result.IsForbidden ? Forbid() : BadRequest(new { message = result.Error });

        return CreatedAtAction(nameof(GetPlanById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Updates an existing subscription plan tier (Admin only).
    /// </summary>
    [HttpPut("plans/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePlan(int id, [FromBody] UpdateSubscriptionPlanDto dto)
    {
        var caller = User.GetUserContext();
        var result = await _subscriptionService.UpdatePlanAsync(id, dto, caller);

        if (!result.Succeeded)
            return result.IsForbidden ? Forbid() : BadRequest(new { message = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Deactivates a plan, preventing new signups while preserving existing subscribers (Admin only).
    /// </summary>
    [HttpDelete("plans/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivatePlan(int id)
    {
        var caller = User.GetUserContext();
        var result = await _subscriptionService.DeactivatePlanAsync(id, caller);

        if (!result.Succeeded)
            return result.IsForbidden ? Forbid() : BadRequest(new { message = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Manually assigns a subscription to a user with staff cash recording or complimentary VIP trial (Admin only).
    /// </summary>
    [HttpPost("admin/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminAssignSubscription([FromBody] AdminAssignSubscriptionDto dto)
    {
        var caller = User.GetUserContext();
        var result = await _subscriptionService.AdminAssignSubscriptionAsync(dto, caller);

        if (!result.Succeeded)
            return result.IsForbidden ? Forbid() : BadRequest(new { message = result.Error });

        return Ok(result.Data);
    }
}

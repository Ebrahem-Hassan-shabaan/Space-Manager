using System.ComponentModel.DataAnnotations;
using SpaceManager.Enums;

namespace SpaceManager.DTOs.Subscriptions;

/// <summary>
/// Data transfer object used by platform administrators to assign a membership subscription directly to a user.
/// </summary>
public class AdminAssignSubscriptionDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int PlanId { get; set; }

    /// <summary>
    /// Cash paid at reception, or Complimentary/Trial.
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    public bool IsAutoRenew { get; set; } = false;

    /// <summary>
    /// If true, waives subscription invoice (e.g. promotional/complimentary trial).
    /// </summary>
    public bool IsComplimentary { get; set; } = false;
}

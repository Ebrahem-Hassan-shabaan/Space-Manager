using System.ComponentModel.DataAnnotations;
using SpaceManager.Enums;

namespace SpaceManager.DTOs.Subscriptions;

/// <summary>
/// Data transfer object used by a member to self-subscribe to a plan using an online payment method.
/// </summary>
public class SubscribeRequestDto
{
    [Required]
    public int PlanId { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;

    public bool IsAutoRenew { get; set; } = true;

    /// <summary>
    /// Simulated or actual card number. Suffixes 0002, 0003, 0005 trigger gateway test decline scenarios.
    /// </summary>
    public string? CardNumber { get; set; }

    public string? CardholderName { get; set; }

    public string? ExpiryMonthYear { get; set; }

    public string? Cvv { get; set; }

    /// <summary>
    /// Optional gateway token (e.g. DECLINE_INSUFFICIENT, DECLINE_EXPIRED, auto).
    /// </summary>
    public string? PaymentToken { get; set; }
}

using System.Collections.Generic;
using SpaceManager.Enums;

namespace SpaceManager.DTOs.Payments;

/// <summary>
/// Encapsulates the parameters required to execute an external charge via a payment gateway.
/// </summary>
/// <param name="Amount">The total financial amount to charge.</param>
/// <param name="Currency">The three-letter ISO currency code (defaults to "EGP").</param>
/// <param name="PaymentMethod">The payment method being processed.</param>
/// <param name="PaymentToken">The gateway-specific payment method token (e.g. Stripe "pm_...", or mock test token).</param>
/// <param name="CustomerEmail">Optional email address of the customer for gateway receipts.</param>
/// <param name="Description">Optional transaction description passed to the gateway.</param>
/// <param name="Metadata">Optional key-value metadata attached to the gateway transaction.</param>
public record PaymentGatewayRequest(
    decimal Amount,
    string Currency = "EGP",
    PaymentMethod PaymentMethod = PaymentMethod.CreditCard,
    string PaymentToken = "",
    string? CustomerEmail = null,
    string? Description = null,
    Dictionary<string, string>? Metadata = null
);

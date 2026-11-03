using System.Threading.Tasks;
using SpaceManager.DTOs.Payments;
using SpaceManager.Enums;

namespace SpaceManager.Services.Interfaces;

/// <summary>
/// Defines the contract for payment gateway integrations (e.g. Stripe, Paymob, or local mock simulators),
/// enabling the Self-Selecting Strategy Pattern across diverse payment rails.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Evaluates whether this gateway implementation supports the specified payment method.
    /// </summary>
    /// <param name="method">The payment method being evaluated.</param>
    /// <returns>True if this gateway handles the method; otherwise, false.</returns>
    bool CanHandle(PaymentMethod method);

    /// <summary>
    /// Charges the specified amount against the external payment gateway.
    /// </summary>
    /// <param name="request">The gateway charge parameters.</param>
    /// <returns>The normalized outcome of the charge attempt.</returns>
    Task<PaymentGatewayResult> ChargeAsync(PaymentGatewayRequest request);

    /// <summary>
    /// Issues a refund for a previously captured transaction through the payment gateway.
    /// </summary>
    /// <param name="transactionReference">The external transaction reference originally returned by the gateway.</param>
    /// <param name="amount">The amount to refund.</param>
    /// <returns>The normalized outcome of the refund attempt.</returns>
    Task<PaymentGatewayResult> RefundAsync(string transactionReference, decimal amount);
}

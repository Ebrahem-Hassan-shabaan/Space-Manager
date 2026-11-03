using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpaceManager.DTOs.Payments;
using SpaceManager.Enums;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Services.Implementations;

/// <summary>
/// Development and testing implementation of <see cref="IPaymentGateway"/> that simulates
/// external banking authorization, declines, and refunds without connecting to live payment networks.
/// </summary>
public class MockPaymentGateway : IPaymentGateway
{
    private readonly ILogger<MockPaymentGateway> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockPaymentGateway"/> class.
    /// </summary>
    /// <param name="logger">The application logger.</param>
    public MockPaymentGateway(ILogger<MockPaymentGateway> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public bool CanHandle(PaymentMethod method) =>
        method == PaymentMethod.CreditCard || method == PaymentMethod.DebitCard;

    /// <inheritdoc />
    public Task<PaymentGatewayResult> ChargeAsync(PaymentGatewayRequest request)
    {
        _logger.LogInformation("Simulating payment gateway charge of {Amount:C} via {Method} with token: {Token}",
            request.Amount, request.PaymentMethod, request.PaymentToken);

        var token = request.PaymentToken?.Trim() ?? string.Empty;

        // Decline when no token or card credential is provided
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Mock gateway simulation: Card declined due to missing card credentials or token.");
            return Task.FromResult(PaymentGatewayResult.Failed("Card declined: Missing card number or payment token.", "missing_card_credentials"));
        }

        // Simulate Insufficient Funds decline
        if (token.Contains("DECLINE_INSUFFICIENT", StringComparison.OrdinalIgnoreCase)
            || token.Contains("chargeCustomerFail", StringComparison.OrdinalIgnoreCase)
            || token.EndsWith("0002"))
        {
            _logger.LogWarning("Mock gateway simulation: Card declined due to insufficient funds.");
            return Task.FromResult(PaymentGatewayResult.Failed("Card declined: Insufficient funds.", "insufficient_funds"));
        }

        // Simulate Expired Card decline
        if (token.Contains("DECLINE_EXPIRED", StringComparison.OrdinalIgnoreCase)
            || token.Contains("expired", StringComparison.OrdinalIgnoreCase)
            || token.EndsWith("0003"))
        {
            _logger.LogWarning("Mock gateway simulation: Card declined because card is expired.");
            return Task.FromResult(PaymentGatewayResult.Failed("Card declined: Card has expired.", "card_expired"));
        }

        // Simulate Fraud Suspected decline
        if (token.Contains("DECLINE_FRAUD", StringComparison.OrdinalIgnoreCase)
            || token.EndsWith("0005"))
        {
            _logger.LogWarning("Mock gateway simulation: Card declined due to suspected fraud.");
            return Task.FromResult(PaymentGatewayResult.Failed("Card declined: Suspected fraudulent transaction.", "fraud_suspected"));
        }

        // Approved simulation: generate external transaction reference
        var txnRef = $"MOCK-TXN-{Guid.NewGuid():N}"[..21].ToUpperInvariant();
        _logger.LogInformation("Mock gateway simulation: Charge approved with reference {TransactionReference}", txnRef);

        return Task.FromResult(PaymentGatewayResult.Success(txnRef));
    }

    /// <inheritdoc />
    public Task<PaymentGatewayResult> RefundAsync(string transactionReference, decimal amount)
    {
        _logger.LogInformation("Simulating payment gateway refund of {Amount:C} for transaction {TransactionReference}",
            amount, transactionReference);

        if (transactionReference.Contains("FAIL_REFUND", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Mock gateway simulation: Refund declined.");
            return Task.FromResult(PaymentGatewayResult.Failed("Simulated gateway refund failure.", "refund_declined"));
        }

        var refundRef = $"MOCK-REF-{Guid.NewGuid():N}"[..21].ToUpperInvariant();
        _logger.LogInformation("Mock gateway simulation: Refund approved with reference {RefundReference}", refundRef);

        return Task.FromResult(PaymentGatewayResult.Success(refundRef));
    }
}

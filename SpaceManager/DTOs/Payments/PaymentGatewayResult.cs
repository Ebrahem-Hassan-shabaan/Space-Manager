namespace SpaceManager.DTOs.Payments;

/// <summary>
/// Encapsulates the normalized outcome of an external payment gateway charge or refund transaction.
/// </summary>
/// <param name="IsSuccess">Indicates whether the gateway transaction was approved and settled.</param>
/// <param name="TransactionReference">The unique transaction or charge reference issued by the gateway.</param>
/// <param name="ErrorMessage">The decline or error message provided by the gateway if unsuccessful.</param>
/// <param name="ErrorCode">The standardized error or decline code returned by the payment processor.</param>
public record PaymentGatewayResult(
    bool IsSuccess,
    string? TransactionReference,
    string? ErrorMessage = null,
    string? ErrorCode = null
)
{
    /// <summary>
    /// Creates a successful gateway transaction outcome.
    /// </summary>
    /// <param name="transactionReference">The external charge reference from the gateway.</param>
    /// <returns>A successful <see cref="PaymentGatewayResult"/>.</returns>
    public static PaymentGatewayResult Success(string transactionReference) =>
        new(true, transactionReference);

    /// <summary>
    /// Creates a declined or failed gateway transaction outcome.
    /// </summary>
    /// <param name="errorMessage">The human-readable decline reason.</param>
    /// <param name="errorCode">Optional normalized gateway error code.</param>
    /// <returns>A failed <see cref="PaymentGatewayResult"/>.</returns>
    public static PaymentGatewayResult Failed(string errorMessage, string? errorCode = null) =>
        new(false, null, errorMessage, errorCode);
}

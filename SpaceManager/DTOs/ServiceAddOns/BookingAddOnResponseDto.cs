namespace SpaceManager.DTOs.ServiceAddOns;

/// <summary>
/// Data transfer object representing an add-on item associated with a confirmed workspace booking.
/// </summary>
/// <param name="ServiceAddOnId">The unique identifier of the service add-on.</param>
/// <param name="ServiceName">The name of the service.</param>
/// <param name="UnitPrice">The unit price at the time of reservation.</param>
/// <param name="Quantity">The quantity reserved.</param>
/// <param name="TotalPrice">The total line item price (Quantity * UnitPrice).</param>
public record BookingAddOnResponseDto(
    int ServiceAddOnId,
    string ServiceName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice
);

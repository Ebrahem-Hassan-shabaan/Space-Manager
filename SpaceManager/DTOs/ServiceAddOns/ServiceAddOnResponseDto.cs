namespace SpaceManager.DTOs.ServiceAddOns;

/// <summary>
/// Data transfer object representing a bookable service add-on.
/// </summary>
/// <param name="Id">The unique identifier of the service add-on.</param>
/// <param name="Name">The name of the service add-on.</param>
/// <param name="Price">The price charged for the service.</param>
/// <param name="IsAvailable">Indicates whether the service is currently offered and available for booking.</param>
public record ServiceAddOnResponseDto(
    int Id,
    string Name,
    decimal Price,
    bool IsAvailable
);

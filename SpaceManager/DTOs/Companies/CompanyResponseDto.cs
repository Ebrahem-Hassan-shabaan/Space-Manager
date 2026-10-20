namespace SpaceManager.DTOs.Companies;

/// <summary>
/// Data transfer object representing a company profile.
/// </summary>
/// <param name="Id">The unique identifier of the company.</param>
/// <param name="Name">The legal or registered business name.</param>
/// <param name="TaxRegistrationNumber">The official tax or commercial registration number.</param>
/// <param name="BillingAddress">The primary billing street address.</param>
/// <param name="ContactEmail">The official corporate contact/invoicing email.</param>
/// <param name="CreatedAt">The timestamp when the company was registered in UTC.</param>
/// <param name="MemberCount">The total number of active employees/members currently associated with the company.</param>
public record CompanyResponseDto(
    int Id,
    string Name,
    string TaxRegistrationNumber,
    string BillingAddress,
    string ContactEmail,
    DateTime CreatedAt,
    int MemberCount = 0
);
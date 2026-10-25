using SpaceManager.Enums;

namespace SpaceManager.DTOs.Invoices;

/// <summary>
/// Query parameters for filtering invoices.
/// </summary>
public record InvoiceFilterDto(
    InvoiceStatus? Status = null,
    int? CompanyId = null,
    string? UserId = null,
    int? WorkspaceBookingId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
);

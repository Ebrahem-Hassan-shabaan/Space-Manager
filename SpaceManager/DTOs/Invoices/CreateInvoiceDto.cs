using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.Invoices;

/// <summary>
/// Data transfer object used to create an invoice.
/// </summary>
public record CreateInvoiceDto(
    [Range(0.01, 10000000, ErrorMessage = "Amount must be greater than zero.")] decimal Amount,
    [Required] DateTime DueDate,
    string? UserId = null,
    int? CompanyId = null,
    int? WorkspaceBookingId = null
);

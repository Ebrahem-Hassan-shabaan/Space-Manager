using SpaceManager.DTOs.Invoices;
using SpaceManager.DTOs.Payments;
using SpaceManager.Models;
using System;
using System.Linq;

namespace SpaceManager.Mappings;

/// <summary>
/// Pure static mapping extensions projecting <see cref="Invoice"/> entities to strongly typed DTOs.
/// </summary>
public static class InvoiceMappings
{
    /// <summary>
    /// Projects an <see cref="Invoice"/> entity to an <see cref="InvoiceResponseDto"/> with calculated payment totals.
    /// Optionally includes payment transaction history.
    /// </summary>
    /// <param name="i">The invoice entity to project.</param>
    /// <param name="includePayments">Whether to include child payment transactions.</param>
    /// <returns>A populated <see cref="InvoiceResponseDto"/>.</returns>
    public static InvoiceResponseDto ToResponseDto(this Invoice i, bool includePayments = false)
    {
        var amountPaid = i.Payments.Sum(p => p.Amount);
        var remaining = Math.Max(0m, i.Amount - amountPaid);

        return new(
            i.Id,
            i.Amount,
            i.IssuedDate,
            i.DueDate,
            i.Status,
            i.UserId,
            i.User?.FullName,
            i.User?.Email,
            i.CompanyId,
            i.Company?.Name,
            i.WorkspaceBookingId,
            amountPaid,
            remaining,
            includePayments ? i.Payments.Select(p => p.ToResponseDto()).ToList() : null
        );
    }
}

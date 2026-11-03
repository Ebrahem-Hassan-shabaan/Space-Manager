using SpaceManager.DTOs.Invoices;
using SpaceManager.DTOs.Payments;
using SpaceManager.Enums;
using SpaceManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceManager.Mappings;

/// <summary>
/// Pure static mapping extensions projecting <see cref="Invoice"/> entities to strongly typed DTOs and vice versa.
/// </summary>
public static class InvoiceMappings
{
    /// <summary>
    /// Projects a <see cref="CreateInvoiceDto"/> creation payload to a new <see cref="Invoice"/> entity in <see cref="InvoiceStatus.Issued"/> status.
    /// </summary>
    /// <param name="dto">The invoice creation data transfer object.</param>
    /// <returns>A newly instantiated <see cref="Invoice"/> ready for persistence.</returns>
    public static Invoice ToEntity(this CreateInvoiceDto dto) =>
        new()
        {
            Amount = dto.Amount,
            IssuedDate = DateTime.UtcNow,
            DueDate = dto.DueDate,
            Status = InvoiceStatus.Issued,
            UserId = dto.UserId,
            CompanyId = dto.CompanyId,
            WorkspaceBookingId = dto.WorkspaceBookingId
        };

    /// <summary>
    /// Projects an <see cref="Invoice"/> entity to an <see cref="InvoiceResponseDto"/> with calculated payment totals and all payment records.
    /// </summary>
    /// <param name="i">The invoice entity to project.</param>
    /// <returns>A populated <see cref="InvoiceResponseDto"/>.</returns>
    public static InvoiceResponseDto ToResponseDto(this Invoice i)
    {
        var paymentsList = i.Payments != null && i.Payments.Count > 0
            ? i.Payments.Select(p => p.ToResponseDto()).ToList()
            : new List<PaymentResponseDto>();

        var amountPaid = paymentsList.Sum(p => p.Amount);
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
            paymentsList
        );
    }
}

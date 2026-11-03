using Microsoft.EntityFrameworkCore;
using SpaceManager.Common;
using SpaceManager.Data;
using SpaceManager.DTOs.Invoices;
using SpaceManager.Enums;
using SpaceManager.Mappings;
using SpaceManager.Models;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Services.Implementations;

/// <summary>
/// Implements billing invoice generation, multi-tenant query scoping, and lifecycle management.
/// </summary>
public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public InvoiceService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InvoiceResponseDto>> GetAllAsync(CurrentUserContext caller, InvoiceFilterDto? filter = null)
    {
        var query = _context.Invoices.AsNoTracking();

        if (caller.IsRegularUser)
        {
            query = query.Where(i => i.UserId == caller.UserId);
        }
        else if (caller.IsCompanyAdmin && caller.CompanyId.HasValue)
        {
            query = query.Where(i => i.CompanyId == caller.CompanyId);
        }

        if (filter != null)
        {
            query = query.Where(i =>
                (!filter.Status.HasValue || i.Status == filter.Status.Value) &&
                (!filter.CompanyId.HasValue || i.CompanyId == filter.CompanyId.Value) &&
                (string.IsNullOrEmpty(filter.UserId) || i.UserId == filter.UserId) &&
                (!filter.WorkspaceBookingId.HasValue || i.WorkspaceBookingId == filter.WorkspaceBookingId.Value) &&
                (!filter.FromDate.HasValue || i.IssuedDate >= filter.FromDate.Value) &&
                (!filter.ToDate.HasValue || i.IssuedDate <= filter.ToDate.Value)
            );
        }

        var invoices = await query
            .Include(i => i.User)
            .Include(i => i.Company)
            .Include(i => i.Payments)
            .OrderByDescending(i => i.IssuedDate)
            .ToListAsync();

        return invoices.Select(i => i.ToResponseDto());
    }

    /// <inheritdoc />
    public async Task<ServiceResult<InvoiceResponseDto>> GetByIdAsync(int id, CurrentUserContext caller)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.User)
            .Include(i => i.Company)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
            return ServiceResult<InvoiceResponseDto>.Failure("Invoice not found.");

        var isAuthorized = caller.IsAdmin
            || (caller.IsCompanyAdmin && caller.CompanyId.HasValue && caller.CompanyId == invoice.CompanyId)
            || (caller.UserId == invoice.UserId);

        if (!isAuthorized)
            return ServiceResult<InvoiceResponseDto>.Forbidden();

        return ServiceResult<InvoiceResponseDto>.Success(invoice.ToResponseDto());
    }

    /// <inheritdoc />
    public async Task<ServiceResult> CancelAsync(int id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Payments)
            .Include(i => i.WorkspaceBooking)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
            return ServiceResult.Failure("Invoice not found.");

        if (invoice.Status == InvoiceStatus.Cancelled)
            return ServiceResult.Failure("Invoice is already cancelled.");

        if (invoice.Status == InvoiceStatus.Paid)
            return ServiceResult.Failure("Cannot cancel a paid invoice. A refund must be issued instead.");

        if (invoice.Payments.Any(p => p.Amount > 0))
            return ServiceResult.Failure("Cannot cancel an invoice with recorded payments.");

        invoice.Status = InvoiceStatus.Cancelled;

        if (invoice.WorkspaceBooking != null && invoice.WorkspaceBooking.Status == BookingStatus.PendingPayment)
        {
            invoice.WorkspaceBooking.Status = BookingStatus.Cancelled;
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> CancelForBookingAsync(int bookingId)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.WorkspaceBookingId == bookingId);

        if (invoice == null)
            return ServiceResult.Success();

        if (invoice.Status == InvoiceStatus.Issued)
        {
            invoice.Status = InvoiceStatus.Cancelled;
            await _context.SaveChangesAsync();
        }

        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult<InvoiceResponseDto>> CreateForBookingAsync(CreateInvoiceDto dto)
    {
        if (dto.Amount <= 0)
            return ServiceResult<InvoiceResponseDto>.Failure("Invoice amount must be greater than zero.");

        var invoice = dto.ToEntity();
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(invoice.UserId))
        {
            await _context.Entry(invoice).Reference(i => i.User).LoadAsync();
        }

        if (invoice.CompanyId.HasValue)
        {
            await _context.Entry(invoice).Reference(i => i.Company).LoadAsync();
        }

        return ServiceResult<InvoiceResponseDto>.Success(invoice.ToResponseDto());
    }

    /// <inheritdoc />
    public async Task<ServiceResult<InvoiceResponseDto>> CreateManualAsync(CreateInvoiceDto dto)
    {
        if (dto.Amount <= 0)
            return ServiceResult<InvoiceResponseDto>.Failure("Invoice amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(dto.UserId) && !dto.CompanyId.HasValue)
            return ServiceResult<InvoiceResponseDto>.Failure("An invoice must be assigned to either a user or a company.");

        if (!string.IsNullOrWhiteSpace(dto.UserId))
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
                return ServiceResult<InvoiceResponseDto>.Failure("Specified debtor user was not found.");
        }

        if (dto.CompanyId.HasValue)
        {
            var companyExists = await _context.Companies.AnyAsync(c => c.Id == dto.CompanyId.Value);
            if (!companyExists)
                return ServiceResult<InvoiceResponseDto>.Failure("Specified company was not found.");
        }

        var invoice = dto.ToEntity();
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(invoice.UserId))
        {
            await _context.Entry(invoice).Reference(i => i.User).LoadAsync();
        }

        if (invoice.CompanyId.HasValue)
        {
            await _context.Entry(invoice).Reference(i => i.Company).LoadAsync();
        }

        return ServiceResult<InvoiceResponseDto>.Success(invoice.ToResponseDto());
    }
}

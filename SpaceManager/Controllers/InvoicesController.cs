using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceManager.Common;
using SpaceManager.DTOs.Invoices;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Controllers;

/// <summary>
/// Provides multi-tenant endpoints for viewing, issuing, and managing financial invoices.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoicesController"/> class.
    /// </summary>
    /// <param name="invoiceService">The invoice business service.</param>
    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    /// <summary>
    /// Retrieves a filtered list of invoices scoped by caller permissions.
    /// Admins view platform-wide billing; Company Admins view company records; Members view personal accounts.
    /// </summary>
    /// <param name="filter">Optional filtering parameters for invoice status and due date ranges.</param>
    /// <returns>A collection of matching invoice response DTOs.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] InvoiceFilterDto filter)
    {
        var caller = User.GetUserContext();
        var invoices = await _invoiceService.GetAllAsync(caller, filter);
        return Ok(invoices);
    }

    /// <summary>
    /// Retrieves detailed invoice information by identifier, including calculated balances and payment history.
    /// Enforces multi-tenant ownership validation.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice.</param>
    /// <returns>The populated invoice response DTO.</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var caller = User.GetUserContext();
        var result = await _invoiceService.GetByIdAsync(id, caller);

        if (!result.Succeeded)
        {
            if (result.IsForbidden)
                return Forbid();

            return NotFound(new { message = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Manually generates an ad-hoc or corporate invoice. Restricted to platform administrators.
    /// </summary>
    /// <param name="dto">The invoice creation data payload.</param>
    /// <returns>The created invoice response DTO with an HTTP 201 Created status.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateManual([FromBody] CreateInvoiceDto dto)
    {
        var result = await _invoiceService.CreateManualAsync(dto);

        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Voids an unpaid invoice and releases any linked reservation hold. Restricted to platform administrators.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice to cancel.</param>
    /// <returns>An HTTP 204 No Content response upon successful cancellation.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _invoiceService.CancelAsync(id);

        if (!result.Succeeded)
        {
            if (result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { message = result.Error });

            return BadRequest(new { message = result.Error });
        }

        return NoContent();
    }
}

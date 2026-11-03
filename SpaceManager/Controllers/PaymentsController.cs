using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpaceManager.Common;
using SpaceManager.DTOs.Payments;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Controllers;

/// <summary>
/// Provides endpoints for processing financial checkouts, viewing payment receipts,
/// issuing administrative refunds, and handling asynchronous payment gateway webhooks.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentsController"/> class.
    /// </summary>
    /// <param name="paymentService">The payment business service.</param>
    /// <param name="logger">The application logger.</param>
    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Processes a financial payment transaction against an invoice.
    /// Online payments are routed through the registered payment gateway strategy.
    /// Cash payments are restricted strictly to platform administrators.
    /// </summary>
    /// <param name="dto">The payment transaction payload.</param>
    /// <returns>The generated payment receipt DTO with an HTTP 201 Created status.</returns>
    [HttpPost]
    public async Task<IActionResult> Process([FromBody] ProcessPaymentDto dto)
    {
        var caller = User.GetUserContext();
        var result = await _paymentService.ProcessAsync(dto, caller);

        if (!result.Succeeded)
        {
            if (result.IsForbidden)
                return Forbid();

            if (result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { message = result.Error });

            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Retrieves a specific payment transaction receipt by identifier. Enforces tenant ownership.
    /// </summary>
    /// <param name="id">The unique identifier of the payment transaction.</param>
    /// <returns>The payment transaction receipt DTO.</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var caller = User.GetUserContext();
        var result = await _paymentService.GetByIdAsync(id, caller);

        if (!result.Succeeded)
        {
            if (result.IsForbidden)
                return Forbid();

            return NotFound(new { message = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves all recorded payment receipts (including partial settlements and refund compensations) for an invoice.
    /// </summary>
    /// <param name="invoiceId">The unique identifier of the invoice.</param>
    /// <returns>A collection of payment receipt DTOs.</returns>
    [HttpGet("invoice/{invoiceId:int}")]
    public async Task<IActionResult> GetByInvoiceId(int invoiceId)
    {
        var caller = User.GetUserContext();
        var result = await _paymentService.GetByInvoiceIdAsync(invoiceId, caller);

        if (!result.Succeeded)
        {
            if (result.IsForbidden)
                return Forbid();

            return NotFound(new { message = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves the financial payment audit log scoped by caller permissions.
    /// Admins see platform-wide revenue; Company Admins see corporate transactions; Members see personal receipts.
    /// </summary>
    /// <returns>A collection of payment receipt DTOs.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var caller = User.GetUserContext();
        var payments = await _paymentService.GetAllAsync(caller);
        return Ok(payments);
    }

    /// <summary>
    /// Issues an accounting-compliant refund by recording a compensating negative transaction and updating invoice/booking status.
    /// Restricted to platform administrators.
    /// </summary>
    /// <param name="id">The unique identifier of the payment transaction to refund.</param>
    /// <returns>The generated refund transaction receipt DTO.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/refund")]
    public async Task<IActionResult> Refund(int id)
    {
        var result = await _paymentService.RefundAsync(id);

        if (!result.Succeeded)
        {
            if (result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { message = result.Error });

            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Data);
    }

   
}

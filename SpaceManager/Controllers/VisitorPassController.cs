using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceManager.DTOs.VisitorPasses;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Controllers;

/// <summary>
/// Provides API endpoints for managing guest visitor passes.
/// </summary>
[ApiController]
[Route("api/visitorpasses")]
[Authorize]
public class VisitorPassController : ControllerBase
{
    private readonly IVisitorPassService _visitorPassService;

    /// <summary>
    /// Initializes a new instance of the <see cref="VisitorPassController"/> class.
    /// </summary>
    /// <param name="visitorPassService">The visitor pass service instance.</param>
    public VisitorPassController(IVisitorPassService visitorPassService)
    {
        _visitorPassService = visitorPassService;
    }

    /// <summary>
    /// Retrieves visitor passes. Admin users view all passes across the system; regular members view their own passes only.
    /// </summary>
    /// <returns>A collection of visitor pass response DTOs.</returns>
    /// <response code="200">Returns the list of visitor passes.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VisitorPassResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetVisitorPasses()
    {
        if (User.IsInRole("Admin"))
        {
            var visitorPasses = await _visitorPassService.GetAllVisitorPassesAsync();
            return Ok(visitorPasses);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var userPasses = await _visitorPassService.GetUserVisitorPassesAsync(userId);
        return Ok(userPasses);
    }

    /// <summary>
    /// Retrieves details of a specific visitor pass by its unique identifier.
    /// Admins can access any pass; regular members can only retrieve passes they created.
    /// </summary>
    /// <param name="id">The unique identifier of the visitor pass.</param>
    /// <returns>The visitor pass response DTO if found.</returns>
    /// <response code="200">Returns the visitor pass details.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="404">If no visitor pass exists with the specified ID or it does not belong to the caller.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VisitorPassResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVisitorPass(int id)
    {
        var isAdmin = User.IsInRole("Admin");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!isAdmin && string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var visitorPass = await _visitorPassService.GetVisitorPassByIdAsync(id, isAdmin ? null : userId);
        if (visitorPass == null)
        {
            return NotFound(new { message = "Visitor pass not found." });
        }

        return Ok(visitorPass);
    }

    /// <summary>
    /// Creates a new visitor pass linked to a confirmed workspace reservation.
    /// Validates booking ownership and ensures the expected arrival is within the booking time slot.
    /// </summary>
    /// <param name="dto">The visitor pass creation request payload.</param>
    /// <returns>The created visitor pass response DTO.</returns>
    /// <response code="201">Returns the created visitor pass and sets the Location header.</response>
    /// <response code="400">If the booking is cancelled or arrival time is outside the booking interval.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    [HttpPost]
    [ProducesResponseType(typeof(VisitorPassResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateVisitorPass([FromBody] CreateVisitorPassDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var visitorPass = await _visitorPassService.CreateVisitorPassAsync(userId, dto);
            return CreatedAtAction(nameof(GetVisitorPass), new { id = visitorPass.Id }, visitorPass);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Marks a visiting guest as checked in upon arrival at the facility.
    /// Restricted to Admin/Receptionist roles.
    /// </summary>
    /// <param name="id">The unique identifier of the visitor pass to check in.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the guest was successfully checked in.</response>
    /// <response code="400">If the guest is already checked in or the booking was cancelled.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="403">If the user is not an administrator.</response>
    /// <response code="404">If the visitor pass is not found.</response>
    [HttpPatch("{id}/checkin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckInVisitorPass(int id)
    {
        try
        {
            var success = await _visitorPassService.CheckInVisitorPassAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Visitor pass not found." });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Revokes/deletes an existing visitor pass before the guest has checked in.
    /// Admins can revoke any pass; regular members can only revoke passes they created.
    /// </summary>
    /// <param name="id">The unique identifier of the visitor pass to delete.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the visitor pass was successfully deleted.</response>
    /// <response code="400">If the guest has already checked in and cannot be deleted.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="404">If the visitor pass is not found or does not belong to the caller.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVisitorPass(int id)
    {
        var isAdmin = User.IsInRole("Admin");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!isAdmin && string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var success = await _visitorPassService.DeleteVisitorPassAsync(id, isAdmin ? null : userId);
            if (!success)
            {
                return NotFound(new { message = "Visitor pass not found." });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

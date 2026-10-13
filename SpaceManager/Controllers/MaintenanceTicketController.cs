using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceManager.DTOs.Tickets;
using SpaceManager.Enums;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Controllers;

/// <summary>
/// Provides API endpoints for reporting and managing workspace maintenance tickets and discussion updates.
/// </summary>
[ApiController]
[Route("api/maintenancetickets")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class MaintenanceTicketController : ControllerBase
{
    private readonly IMaintenanceTicketService _maintenanceTicketService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MaintenanceTicketController"/> class.
    /// </summary>
    /// <param name="maintenanceTicketService">The maintenance ticket service instance.</param>
    public MaintenanceTicketController(IMaintenanceTicketService maintenanceTicketService)
    {
        _maintenanceTicketService = maintenanceTicketService;
    }

    /// <summary>
    /// Retrieves maintenance tickets with optional status and emergency level filtering.
    /// Admin users retrieve tickets across all users; regular members retrieve their own tickets only.
    /// </summary>
    /// <param name="status">Optional ticket status filter (Open, InProgress, Resolved, Closed).</param>
    /// <param name="emergencyLevel">Optional priority filter (Low, Medium, High, Critical).</param>
    /// <returns>A collection of matching maintenance ticket response DTOs.</returns>
    /// <response code="200">Returns the list of matching tickets.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TicketResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllTickets(
        [FromQuery] TicketStatus? status = null,
        [FromQuery] TicketEmergencyLevel? emergencyLevel = null)
    {
        if (User.IsInRole("Admin"))
        {
            var adminTickets = await _maintenanceTicketService.GetAllMaintenanceTicketsAsync(status, emergencyLevel);
            return Ok(adminTickets);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var userTickets = await _maintenanceTicketService.GetUserMaintenanceTicketsAsync(userId, status, emergencyLevel);
        return Ok(userTickets);
    }

    /// <summary>
    /// Retrieves a specific maintenance ticket by its unique identifier, including its discussion thread.
    /// Admins can access any ticket; regular members can only retrieve tickets they reported.
    /// </summary>
    /// <param name="id">The unique identifier of the maintenance ticket.</param>
    /// <returns>The ticket response DTO if found.</returns>
    /// <response code="200">Returns the ticket details.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="404">If the ticket is not found or does not belong to the caller.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTicketById(int id)
    {
        var isAdmin = User.IsInRole("Admin");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!isAdmin && string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var ticket = await _maintenanceTicketService.GetMaintenanceTicketByIdAsync(id, isAdmin ? null : userId);
        if (ticket == null)
        {
            return NotFound(new { message = "Maintenance ticket not found." });
        }

        return Ok(ticket);
    }

    /// <summary>
    /// Reports a new maintenance issue for a workspace.
    /// Validates workspace existence and initializes the ticket in Open status.
    /// </summary>
    /// <param name="dto">The ticket creation payload containing issue title, description, and emergency level.</param>
    /// <returns>The created maintenance ticket response DTO.</returns>
    /// <response code="201">Returns the created ticket and sets the Location header.</response>
    /// <response code="400">If the workspace does not exist or payload validation fails.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var ticket = await _maintenanceTicketService.CreateMaintenanceTicketAsync(userId, dto);
            return CreatedAtAction(nameof(GetTicketById), new { id = ticket.Id }, ticket);
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
    /// Updates the lifecycle status of an existing maintenance ticket.
    /// Restricted to administrator users.
    /// </summary>
    /// <param name="id">The unique identifier of the maintenance ticket.</param>
    /// <param name="dto">The new status payload.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the ticket status was successfully updated.</response>
    /// <response code="400">If the ticket is already in the specified status.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="403">If the caller is not an administrator.</response>
    /// <response code="404">If the maintenance ticket is not found.</response>
    [HttpPatch("{id}/status")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTicketStatus(int id, [FromBody] UpdateTicketStatusDto dto)
    {
        try
        {
            var success = await _maintenanceTicketService.UpdateMaintenanceTicketStatusAsync(id, dto);
            if (!success)
            {
                return NotFound(new { message = "Maintenance ticket not found." });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes or cancels a maintenance ticket.
    /// Regular members can only delete their own tickets while in Open status. Admins can delete any ticket.
    /// </summary>
    /// <param name="id">The unique identifier of the maintenance ticket to delete.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the ticket was successfully deleted.</response>
    /// <response code="400">If regular member attempts to delete a ticket not in Open status.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="404">If the ticket is not found or does not belong to the caller.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var isAdmin = User.IsInRole("Admin");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!isAdmin && string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var success = await _maintenanceTicketService.DeleteMaintenanceTicketAsync(id, isAdmin ? null : userId);
            if (!success)
            {
                return NotFound(new { message = "Maintenance ticket not found." });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Adds a comment or progress note to an existing maintenance ticket's discussion thread.
    /// Automatically flags comments submitted by administrators as staff replies.
    /// </summary>
    /// <param name="id">The unique identifier of the maintenance ticket.</param>
    /// <param name="dto">The update payload containing the message.</param>
    /// <returns>The created ticket update response DTO.</returns>
    /// <response code="201">Returns the created update comment.</response>
    /// <response code="400">If the ticket is closed or message is invalid.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="404">If the ticket is not found or does not belong to the caller.</response>
    [HttpPost("{id}/updates")]
    [ProducesResponseType(typeof(TicketUpdateResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTicketUpdate(int id, [FromBody] AddTicketUpdateDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var isAdmin = User.IsInRole("Admin");

        try
        {
            var ticketUpdate = await _maintenanceTicketService.AddTicketUpdateAsync(id, userId, isAdmin, dto);
            return CreatedAtAction(nameof(GetTicketById), new { id = id }, ticketUpdate);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("Ticket not found"))
            {
                return NotFound(new { message = "Maintenance ticket not found." });
            }

            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes an individual ticket update comment from a maintenance ticket's thread.
    /// Restricted to administrator moderation use.
    /// </summary>
    /// <param name="id">The unique identifier of the maintenance ticket.</param>
    /// <param name="updateId">The unique identifier of the ticket update to delete.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the ticket update comment was successfully deleted.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="403">If the caller is not an administrator.</response>
    /// <response code="404">If the ticket update is not found on the specified ticket.</response>
    [HttpDelete("{id}/updates/{updateId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTicketUpdate(int id, int updateId)
    {
        var success = await _maintenanceTicketService.DeleteTicketUpdateAsync(id, updateId);
        if (!success)
        {
            return NotFound(new { message = "Ticket update not found." });
        }

        return NoContent();
    }
}

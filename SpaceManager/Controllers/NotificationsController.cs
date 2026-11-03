using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceManager.Common;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Controllers;

/// <summary>
/// Provides REST API endpoints for viewing and acknowledging in-app notifications,
/// including a lightweight unread count endpoint optimized for frontend polling loops.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Retrieves paginated notifications belonging to the authenticated caller.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var caller = User.GetUserContext();
        var result = await _notificationService.GetMyNotificationsAsync(caller, page, pageSize);

        if (!result.Succeeded)
            return result.IsForbidden ? Forbid() : BadRequest(new { message = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Returns the total unread notifications count for the caller.
    /// Optimized for 30-second periodic polling by the frontend navbar bell.
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var caller = User.GetUserContext();
        var result = await _notificationService.GetUnreadCountAsync(caller.UserId);

        if (!result.Succeeded)
            return BadRequest(new { message = result.Error });

        return Ok(new { count = result.Data });
    }

    /// <summary>
    /// Marks a single notification as read.
    /// </summary>
    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var caller = User.GetUserContext();
        var result = await _notificationService.MarkAsReadAsync(id, caller);

        if (!result.Succeeded)
            return result.IsForbidden ? Forbid() : BadRequest(new { message = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Marks all unread notifications of the caller as read.
    /// </summary>
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var caller = User.GetUserContext();
        var result = await _notificationService.MarkAllAsReadAsync(caller);

        if (!result.Succeeded)
            return result.IsForbidden ? Forbid() : BadRequest(new { message = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Deletes or dismisses a notification.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var caller = User.GetUserContext();
        var result = await _notificationService.DeleteNotificationAsync(id, caller);

        if (!result.Succeeded)
            return result.IsForbidden ? Forbid() : BadRequest(new { message = result.Error });

        return NoContent();
    }
}

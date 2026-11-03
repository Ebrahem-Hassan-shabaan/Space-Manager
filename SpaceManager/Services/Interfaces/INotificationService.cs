using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SpaceManager.Common;
using SpaceManager.DTOs.Notifications;

namespace SpaceManager.Services.Interfaces;

/// <summary>
/// Orchestrates cross-channel notification dispatching and manages user notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Dispatches a notification across all requested channels (defaults to InApp) via the strategy handlers.
    /// </summary>
    Task<ServiceResult> SendNotificationAsync(CreateNotificationDto dto, CancellationToken ct = default);

    /// <summary>
    /// Retrieves paginated notifications belonging to the authenticated caller, ordered newest first.
    /// </summary>
    Task<ServiceResult<List<NotificationResponseDto>>> GetMyNotificationsAsync(CurrentUserContext caller, int page = 1, int pageSize = 20);

    /// <summary>
    /// Queries the total count of unread notifications for the specified user (optimized for polling).
    /// </summary>
    Task<ServiceResult<int>> GetUnreadCountAsync(string userId);

    /// <summary>
    /// Marks a single notification as read if owned by the caller.
    /// </summary>
    Task<ServiceResult> MarkAsReadAsync(int notificationId, CurrentUserContext caller);

    /// <summary>
    /// Marks all unread notifications of the caller as read.
    /// </summary>
    Task<ServiceResult> MarkAllAsReadAsync(CurrentUserContext caller);

    /// <summary>
    /// Deletes a notification if owned by the caller or if the caller is an administrator.
    /// </summary>
    Task<ServiceResult> DeleteNotificationAsync(int notificationId, CurrentUserContext caller);
}

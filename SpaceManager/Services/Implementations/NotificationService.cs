using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpaceManager.Common;
using SpaceManager.Data;
using SpaceManager.DTOs.Notifications;
using SpaceManager.Enums;
using SpaceManager.Mappings;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Services.Implementations;

/// <summary>
/// Orchestrates cross-channel notification dispatch using the Open/Closed Principle strategy pattern,
/// delegating delivery to registered <see cref="INotificationChannelHandler"/> instances.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IEnumerable<INotificationChannelHandler> _handlers;
    private readonly AppDbContext _db;

    public NotificationService(IEnumerable<INotificationChannelHandler> handlers, AppDbContext db)
    {
        _handlers = handlers;
        _db = db;
    }

    public async Task<ServiceResult> SendNotificationAsync(CreateNotificationDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.UserId))
            return ServiceResult.Failure("Recipient UserId is required.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return ServiceResult.Failure("Notification title cannot be empty.");

        var message = new NotificationMessage(
            dto.UserId,
            dto.Title.Trim(),
            dto.Message?.Trim() ?? string.Empty,
            dto.ActionUrl,
            dto.WorkspaceBookingId,
            dto.CompanyId
        );

        var channels = dto.Channels is { Count: > 0 }
            ? dto.Channels
            : new List<NotificationChannel> { NotificationChannel.InApp };

        foreach (var channel in channels)
        {
            var handler = _handlers.FirstOrDefault(h => h.CanHandle(channel));
            if (handler != null)
            {
                var result = await handler.SendAsync(message, ct);
                if (!result.Succeeded)
                {
                    // Log or handle individual channel failures without breaking the pipeline
                }
            }
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<List<NotificationResponseDto>>> GetMyNotificationsAsync(CurrentUserContext caller, int page = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(caller.UserId))
            return ServiceResult<List<NotificationResponseDto>>.Forbidden();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var notifications = await _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == caller.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = notifications.Select(n => n.ToResponseDto()).ToList();
        return ServiceResult<List<NotificationResponseDto>>.Success(dtos);
    }

    public async Task<ServiceResult<int>> GetUnreadCountAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return ServiceResult<int>.Success(0);

        var count = await _db.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        return ServiceResult<int>.Success(count);
    }

    public async Task<ServiceResult> MarkAsReadAsync(int notificationId, CurrentUserContext caller)
    {
        if (string.IsNullOrWhiteSpace(caller.UserId))
            return ServiceResult.Forbidden();

        var notification = await _db.Notifications.FindAsync(notificationId);
        if (notification == null)
            return ServiceResult.Failure($"Notification #{notificationId} not found.");

        if (notification.UserId != caller.UserId && !caller.IsAdmin)
            return ServiceResult.Forbidden("You do not have permission to modify this notification.");

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _db.SaveChangesAsync();
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> MarkAllAsReadAsync(CurrentUserContext caller)
    {
        if (string.IsNullOrWhiteSpace(caller.UserId))
            return ServiceResult.Forbidden();

        await _db.Notifications
            .Where(n => n.UserId == caller.UserId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteNotificationAsync(int notificationId, CurrentUserContext caller)
    {
        if (string.IsNullOrWhiteSpace(caller.UserId))
            return ServiceResult.Forbidden();

        var notification = await _db.Notifications.FindAsync(notificationId);
        if (notification == null)
            return ServiceResult.Failure($"Notification #{notificationId} not found.");

        if (notification.UserId != caller.UserId && !caller.IsAdmin)
            return ServiceResult.Forbidden("You do not have permission to delete this notification.");

        _db.Notifications.Remove(notification);
        await _db.SaveChangesAsync();

        return ServiceResult.Success();
    }
}

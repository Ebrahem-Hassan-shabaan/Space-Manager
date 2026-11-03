using SpaceManager.DTOs.Notifications;
using SpaceManager.Models;

namespace SpaceManager.Mappings;

/// <summary>
/// Pure static mapping extensions projecting <see cref="Notification"/> entities and DTOs.
/// </summary>
public static class NotificationMappings
{
    public static Notification ToEntity(this CreateNotificationDto dto) =>
        new()
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Message = dto.Message,
            ActionUrl = dto.ActionUrl,
            WorkspaceBookingId = dto.WorkspaceBookingId,
            CompanyId = dto.CompanyId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

    public static Notification ToEntity(this NotificationMessage msg) =>
        new()
        {
            UserId = msg.UserId,
            Title = msg.Title,
            Message = msg.Body,
            ActionUrl = msg.ActionUrl,
            WorkspaceBookingId = msg.WorkspaceBookingId,
            CompanyId = msg.CompanyId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

    public static NotificationResponseDto ToResponseDto(this Notification n) =>
        new()
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            ActionUrl = n.ActionUrl,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            WorkspaceBookingId = n.WorkspaceBookingId,
            CompanyId = n.CompanyId
        };
}

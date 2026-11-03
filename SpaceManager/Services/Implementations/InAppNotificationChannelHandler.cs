using System;
using System.Threading;
using System.Threading.Tasks;
using SpaceManager.Common;
using SpaceManager.Data;
using SpaceManager.DTOs.Notifications;
using SpaceManager.Enums;
using SpaceManager.Mappings;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Services.Implementations;

/// <summary>
/// Persists notification records into the database for in-app alert display and notification bell queries.
/// </summary>
public class InAppNotificationChannelHandler : INotificationChannelHandler
{
    private readonly AppDbContext _db;

    public InAppNotificationChannelHandler(AppDbContext db)
    {
        _db = db;
    }

    public bool CanHandle(NotificationChannel channel) => channel == NotificationChannel.InApp;

    public async Task<ServiceResult> SendAsync(NotificationMessage message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(message.UserId))
            return ServiceResult.Failure("Cannot deliver in-app notification without a valid UserId recipient.");

        var entity = message.ToEntity();
        _db.Notifications.Add(entity);
        await _db.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}

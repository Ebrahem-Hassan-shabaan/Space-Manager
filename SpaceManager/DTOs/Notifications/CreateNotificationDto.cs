using System.ComponentModel.DataAnnotations;
using SpaceManager.Enums;

namespace SpaceManager.DTOs.Notifications;

/// <summary>
/// Data transfer object used to dispatch notifications through the notification engine.
/// </summary>
public class CreateNotificationDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ActionUrl { get; set; }

    public int? WorkspaceBookingId { get; set; }

    public int? CompanyId { get; set; }

    /// <summary>
    /// The target delivery channels. If null or empty, defaults to InApp.
    /// </summary>
    public List<NotificationChannel>? Channels { get; set; }
}

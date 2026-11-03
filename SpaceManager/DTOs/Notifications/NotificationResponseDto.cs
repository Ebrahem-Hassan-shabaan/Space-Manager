namespace SpaceManager.DTOs.Notifications;

/// <summary>
/// Data transfer object representing a delivered notification item for client rendering.
/// </summary>
public class NotificationResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? WorkspaceBookingId { get; set; }
    public int? CompanyId { get; set; }
}

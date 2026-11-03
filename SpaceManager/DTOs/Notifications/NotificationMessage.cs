namespace SpaceManager.DTOs.Notifications;

/// <summary>
/// Domain carrier object delivered to concrete notification channel handlers (InApp, Email, SMS).
/// </summary>
public record NotificationMessage(
    string UserId,
    string Title,
    string Body,
    string? ActionUrl = null,
    int? WorkspaceBookingId = null,
    int? CompanyId = null
);

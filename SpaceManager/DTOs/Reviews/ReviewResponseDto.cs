namespace SpaceManager.DTOs.Reviews;

/// <summary>
/// Data transfer object representing a submitted review with associated workspace and user details.
/// </summary>
public class ReviewResponseDto
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public int WorkspaceBookingId { get; set; }
    public int WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}

namespace SpaceManager.Models;

/// <summary>
/// Represents a customer rating and qualitative review for a workspace, tied directly to a completed booking.
/// </summary>
public class WorkspaceReview
{
    /// <summary>
    /// Gets or sets the primary key identifier of the review.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the star rating awarded by the user, constrained between 1 and 5.
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Gets or sets optional qualitative feedback or commentary regarding the workspace experience.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets the timestamp indicating when the review was submitted.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the foreign key of the specific completed reservation being reviewed (1:1 relation).
    /// </summary>
    public int WorkspaceBookingId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the reservation being reviewed.
    /// </summary>
    public WorkspaceBooking WorkspaceBooking { get; set; } = null!;

    /// <summary>
    /// Gets or sets the foreign key of the workspace being reviewed.
    /// </summary>
    public int WorkspaceId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the workspace being reviewed.
    /// </summary>
    public Workspace Workspace { get; set; } = null!;

    /// <summary>
    /// Gets or sets the foreign key of the user authoring the review.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the navigation property to the review author.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.Reviews;

/// <summary>
/// Data transfer object used by a customer to submit a workspace rating and review for a completed booking.
/// </summary>
public class CreateReviewDto
{
    /// <summary>
    /// Gets or sets the completed booking ID being reviewed.
    /// </summary>
    [Required]
    public int WorkspaceBookingId { get; set; }

    /// <summary>
    /// Gets or sets the star rating (1 to 5).
    /// </summary>
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
    public int Rating { get; set; }

    /// <summary>
    /// Gets or sets optional qualitative feedback.
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Review comment cannot exceed 1000 characters.")]
    public string? Comment { get; set; }
}

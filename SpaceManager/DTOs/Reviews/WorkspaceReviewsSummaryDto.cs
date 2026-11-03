namespace SpaceManager.DTOs.Reviews;

/// <summary>
/// Data transfer object summarizing aggregate ratings and reviews for a workspace.
/// </summary>
public class WorkspaceReviewsSummaryDto
{
    public int WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new()
    {
        { 5, 0 }, { 4, 0 }, { 3, 0 }, { 2, 0 }, { 1, 0 }
    };
    public List<ReviewResponseDto> Reviews { get; set; } = new();
}

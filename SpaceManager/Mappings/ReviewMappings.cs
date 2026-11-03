using SpaceManager.DTOs.Reviews;
using SpaceManager.Models;

namespace SpaceManager.Mappings;

/// <summary>
/// Pure static mapping extensions projecting <see cref="WorkspaceReview"/> entities and DTOs.
/// </summary>
public static class ReviewMappings
{
    public static WorkspaceReview ToEntity(this CreateReviewDto dto, int workspaceId, string userId) =>
        new()
        {
            WorkspaceBookingId = dto.WorkspaceBookingId,
            WorkspaceId = workspaceId,
            UserId = userId,
            Rating = dto.Rating,
            Comment = dto.Comment?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

    public static ReviewResponseDto ToResponseDto(this WorkspaceReview r) =>
        new()
        {
            Id = r.Id,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            WorkspaceBookingId = r.WorkspaceBookingId,
            WorkspaceId = r.WorkspaceId,
            WorkspaceName = r.Workspace?.Name ?? string.Empty,
            UserId = r.UserId,
            UserName = r.User?.FullName ?? r.User?.UserName ?? "Anonymous"
        };
}

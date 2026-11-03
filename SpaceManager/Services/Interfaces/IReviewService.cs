using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceManager.Common;
using SpaceManager.DTOs.Reviews;

namespace SpaceManager.Services.Interfaces;

/// <summary>
/// Provides business operations for submitting, moderating, and querying customer workspace reviews.
/// </summary>
public interface IReviewService
{
    /// <summary>
    /// Submits a review and star rating for a completed workspace reservation.
    /// </summary>
    Task<ServiceResult<ReviewResponseDto>> SubmitReviewAsync(CreateReviewDto dto, CurrentUserContext caller);

    /// <summary>
    /// Computes aggregated rating metrics and returns paginated reviews for a specific workspace.
    /// </summary>
    Task<ServiceResult<WorkspaceReviewsSummaryDto>> GetWorkspaceReviewsAsync(int workspaceId, int page = 1, int pageSize = 10);

    /// <summary>
    /// Retrieves all reviews authored by the authenticated caller.
    /// </summary>
    Task<ServiceResult<List<ReviewResponseDto>>> GetMyReviewsAsync(CurrentUserContext caller);

    /// <summary>
    /// Checks whether a completed booking has already been reviewed, returning its review if present.
    /// </summary>
    Task<ServiceResult<ReviewResponseDto?>> GetReviewByBookingAsync(int bookingId, CurrentUserContext caller);

    /// <summary>
    /// Deletes a review. Only the review author or platform administrators can delete reviews.
    /// </summary>
    Task<ServiceResult> DeleteReviewAsync(int reviewId, CurrentUserContext caller);
}

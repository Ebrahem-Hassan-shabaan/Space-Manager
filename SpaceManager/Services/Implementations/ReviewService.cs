using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpaceManager.Common;
using SpaceManager.Data;
using SpaceManager.DTOs.Reviews;
using SpaceManager.Enums;
using SpaceManager.Mappings;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Services.Implementations;

/// <summary>
/// Implements business logic for workspace reviews, enforcing completed-booking eligibility and rating calculations.
/// </summary>
public class ReviewService : IReviewService
{
    private readonly AppDbContext _db;

    public ReviewService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<ReviewResponseDto>> SubmitReviewAsync(CreateReviewDto dto, CurrentUserContext caller)
    {
        if (string.IsNullOrWhiteSpace(caller.UserId))
            return ServiceResult<ReviewResponseDto>.Forbidden();

        if (dto.Rating < 1 || dto.Rating > 5)
            return ServiceResult<ReviewResponseDto>.Failure("Rating must be between 1 and 5 stars.");

        var booking = await _db.WorkspaceBookings
            .Include(b => b.Workspace)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == dto.WorkspaceBookingId);

        if (booking == null)
            return ServiceResult<ReviewResponseDto>.Failure($"Reservation #{dto.WorkspaceBookingId} does not exist.");

        if (booking.UserId != caller.UserId && !caller.IsAdmin)
            return ServiceResult<ReviewResponseDto>.Forbidden("You may only review reservations made under your account.");

        if (booking.Status != BookingStatus.Completed)
            return ServiceResult<ReviewResponseDto>.Failure("Only completed reservations can be reviewed. Your visit must be finished first.");

        var alreadyReviewed = await _db.WorkspaceReviews.AnyAsync(r => r.WorkspaceBookingId == dto.WorkspaceBookingId);
        if (alreadyReviewed)
            return ServiceResult<ReviewResponseDto>.Failure("A review has already been submitted for this reservation.");

        var review = dto.ToEntity(booking.WorkspaceId, caller.UserId);
        _db.WorkspaceReviews.Add(review);
        await _db.SaveChangesAsync();

        // Reload navigation properties for response DTO projection
        review.Workspace = booking.Workspace;
        review.User = booking.User;

        return ServiceResult<ReviewResponseDto>.Success(review.ToResponseDto());
    }

    public async Task<ServiceResult<WorkspaceReviewsSummaryDto>> GetWorkspaceReviewsAsync(int workspaceId, int page = 1, int pageSize = 10)
    {
        var workspace = await _db.Workspaces.FindAsync(workspaceId);
        if (workspace == null)
            return ServiceResult<WorkspaceReviewsSummaryDto>.Failure($"Workspace #{workspaceId} not found.");

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var reviewsQuery = _db.WorkspaceReviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Workspace)
            .Where(r => r.WorkspaceId == workspaceId);

        var totalReviews = await reviewsQuery.CountAsync();
        var averageRating = totalReviews > 0 ? await reviewsQuery.AverageAsync(r => r.Rating) : 0.0;

        // Group ratings by score 1..5 for visual breakdown bar charts
        var ratingCounts = await reviewsQuery
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync();

        var distribution = new Dictionary<int, int> { { 5, 0 }, { 4, 0 }, { 3, 0 }, { 2, 0 }, { 1, 0 } };
        foreach (var item in ratingCounts)
        {
            if (distribution.ContainsKey(item.Rating))
                distribution[item.Rating] = item.Count;
        }

        var paginatedReviews = await reviewsQuery
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var summary = new WorkspaceReviewsSummaryDto
        {
            WorkspaceId = workspace.Id,
            WorkspaceName = workspace.Name,
            AverageRating = Math.Round(averageRating, 1),
            TotalReviews = totalReviews,
            RatingDistribution = distribution,
            Reviews = paginatedReviews.Select(r => r.ToResponseDto()).ToList()
        };

        return ServiceResult<WorkspaceReviewsSummaryDto>.Success(summary);
    }

    public async Task<ServiceResult<List<ReviewResponseDto>>> GetMyReviewsAsync(CurrentUserContext caller)
    {
        if (string.IsNullOrWhiteSpace(caller.UserId))
            return ServiceResult<List<ReviewResponseDto>>.Forbidden();

        var reviews = await _db.WorkspaceReviews
            .AsNoTracking()
            .Include(r => r.Workspace)
            .Include(r => r.User)
            .Where(r => r.UserId == caller.UserId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return ServiceResult<List<ReviewResponseDto>>.Success(reviews.Select(r => r.ToResponseDto()).ToList());
    }

    public async Task<ServiceResult<ReviewResponseDto?>> GetReviewByBookingAsync(int bookingId, CurrentUserContext caller)
    {
        if (string.IsNullOrWhiteSpace(caller.UserId))
            return ServiceResult<ReviewResponseDto?>.Forbidden();

        var review = await _db.WorkspaceReviews
            .AsNoTracking()
            .Include(r => r.Workspace)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.WorkspaceBookingId == bookingId);

        if (review == null)
            return ServiceResult<ReviewResponseDto?>.Success(null);

        if (review.UserId != caller.UserId && !caller.IsAdmin)
            return ServiceResult<ReviewResponseDto?>.Forbidden();

        return ServiceResult<ReviewResponseDto?>.Success(review.ToResponseDto());
    }

    public async Task<ServiceResult> DeleteReviewAsync(int reviewId, CurrentUserContext caller)
    {
        if (string.IsNullOrWhiteSpace(caller.UserId))
            return ServiceResult.Forbidden();

        var review = await _db.WorkspaceReviews.FindAsync(reviewId);
        if (review == null)
            return ServiceResult.Failure($"Review #{reviewId} not found.");

        if (review.UserId != caller.UserId && !caller.IsAdmin)
            return ServiceResult.Forbidden("You do not have permission to delete this review.");

        _db.WorkspaceReviews.Remove(review);
        await _db.SaveChangesAsync();

        return ServiceResult.Success();
    }
}

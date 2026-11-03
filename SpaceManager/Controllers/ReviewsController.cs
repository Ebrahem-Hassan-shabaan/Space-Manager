using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceManager.Common;
using SpaceManager.DTOs.Reviews;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Controllers;

/// <summary>
/// Provides REST API endpoints for submitting, reading, and moderating workspace customer reviews.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Submits a rating and qualitative feedback for a completed workspace reservation.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SubmitReview([FromBody] CreateReviewDto dto)
    {
        var caller = User.GetUserContext();
        var result = await _reviewService.SubmitReviewAsync(dto, caller);

        if (!result.Succeeded)
            return result.IsForbidden ? Forbid() : BadRequest(new { message = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves all reviews authored by the authenticated caller.
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReviews()
    {
        var caller = User.GetUserContext();
        var result = await _reviewService.GetMyReviewsAsync(caller);

        if (!result.Succeeded)
            return result.IsForbidden ? Forbid() : BadRequest(new { message = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves the review submitted for a specific completed booking, if present.
    /// </summary>
    [HttpGet("booking/{bookingId:int}")]
    public async Task<IActionResult> GetReviewByBooking(int bookingId)
    {
        var caller = User.GetUserContext();
        var result = await _reviewService.GetReviewByBookingAsync(bookingId, caller);

        if (!result.Succeeded)
            return result.IsForbidden ? Forbid() : BadRequest(new { message = result.Error });

        if (result.Data == null)
            return NotFound(new { message = "No review has been submitted for this reservation." });

        return Ok(result.Data);
    }

    /// <summary>
    /// Deletes a review. Callers can only delete their own reviews unless they possess the Admin role.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var caller = User.GetUserContext();
        var result = await _reviewService.DeleteReviewAsync(id, caller);

        if (!result.Succeeded)
            return result.IsForbidden ? Forbid() : BadRequest(new { message = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Publicly retrieves aggregated ratings and paginated reviews for a given workspace.
    /// </summary>
    [HttpGet("/api/workspaces/{workspaceId:int}/reviews")]
    [AllowAnonymous]
    public async Task<IActionResult> GetWorkspaceReviews(int workspaceId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _reviewService.GetWorkspaceReviewsAsync(workspaceId, page, pageSize);

        if (!result.Succeeded)
            return NotFound(new { message = result.Error });

        return Ok(result.Data);
    }
}

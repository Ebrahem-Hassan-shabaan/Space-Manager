using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceManager.DTOs.Amenities;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Controllers;

/// <summary>
/// Provides API endpoints for managing the amenities catalog and workspace amenity assignments.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AmenitiesController : ControllerBase
{
    private readonly IAmenityService _amenityService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AmenitiesController"/> class.
    /// </summary>
    /// <param name="amenityService">The amenity service instance.</param>
    public AmenitiesController(IAmenityService amenityService)
    {
        _amenityService = amenityService;
    }

    /// <summary>
    /// Retrieves all amenities available in the catalog.
    /// </summary>
    /// <returns>A collection of amenity DTOs.</returns>
    /// <response code="200">Returns the full list of amenities.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AmenityResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var amenities = await _amenityService.GetAllAsync();
        return Ok(amenities);
    }

    /// <summary>
    /// Retrieves details of a specific amenity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the amenity.</param>
    /// <returns>The amenity details DTO if found.</returns>
    /// <response code="200">Returns the requested amenity details.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="404">If no amenity exists with the specified ID.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AmenityResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var amenity = await _amenityService.GetByIdAsync(id);
        if (amenity == null)
        {
            return NotFound(new { message = $"Amenity with ID {id} was not found." });
        }

        return Ok(amenity);
    }

    /// <summary>
    /// Creates a new amenity in the catalog. (Admin restricted)
    /// </summary>
    /// <param name="dto">The creation payload.</param>
    /// <returns>The created amenity DTO with its location header.</returns>
    /// <response code="201">Returns the newly created amenity.</response>
    /// <response code="400">If the input validation fails.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="403">If the caller is not an administrator.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AmenityResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateAmenityDto dto)
    {
        var createdAmenity = await _amenityService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = createdAmenity.Id }, createdAmenity);
    }

    /// <summary>
    /// Updates an existing amenity by its unique identifier. (Admin restricted)
    /// </summary>
    /// <param name="id">The unique identifier of the amenity to update.</param>
    /// <param name="dto">The updated amenity payload.</param>
    /// <returns>The updated amenity DTO.</returns>
    /// <response code="200">Returns the updated amenity details.</response>
    /// <response code="400">If the input validation fails.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="403">If the caller is not an administrator.</response>
    /// <response code="404">If no amenity exists with the specified ID.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AmenityResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAmenityDto dto)
    {
        var updatedAmenity = await _amenityService.UpdateAsync(id, dto);
        if (updatedAmenity == null)
        {
            return NotFound(new { message = $"Amenity with ID {id} was not found." });
        }

        return Ok(updatedAmenity);
    }

    /// <summary>
    /// Deletes an amenity by its unique identifier. (Admin restricted)
    /// </summary>
    /// <param name="id">The unique identifier of the amenity to delete.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the amenity was successfully deleted.</response>
    /// <response code="400">If the amenity is actively assigned to one or more workspaces.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="403">If the caller is not an administrator.</response>
    /// <response code="404">If no amenity exists with the specified ID.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _amenityService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Amenity with ID {id} was not found." });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves all amenities assigned to a specific workspace.
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace.</param>
    /// <returns>A collection of workspace amenity DTOs.</returns>
    /// <response code="200">Returns all amenities associated with the workspace.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    [HttpGet("workspace/{workspaceId:int}")]
    [ProducesResponseType(typeof(IEnumerable<WorkspaceAmenityResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByWorkspace(int workspaceId)
    {
        var amenities = await _amenityService.GetByWorkspaceIdAsync(workspaceId);
        return Ok(amenities);
    }

    /// <summary>
    /// Assigns an amenity to a workspace or updates its operational status. (Admin restricted)
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace.</param>
    /// <param name="dto">The assignment payload.</param>
    /// <returns>An OK result on success.</returns>
    /// <response code="200">If the amenity was successfully assigned or updated.</response>
    /// <response code="400">If the workspace or amenity does not exist.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="403">If the caller is not an administrator.</response>
    [HttpPost("workspace/{workspaceId:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignToWorkspace(int workspaceId, [FromBody] AssignWorkspaceAmenityDto dto)
    {
        try
        {
            var success = await _amenityService.AssignToWorkspaceAsync(workspaceId, dto);
            if (!success)
            {
                return BadRequest(new { message = "Failed to assign amenity to workspace." });
            }

            return Ok(new { message = "Amenity assigned to workspace successfully." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Removes an amenity assignment from a workspace. (Admin restricted)
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace.</param>
    /// <param name="amenityId">The unique identifier of the amenity to remove.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the assignment was successfully removed.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="403">If the caller is not an administrator.</response>
    /// <response code="404">If the assignment was not found.</response>
    [HttpDelete("workspace/{workspaceId:int}/{amenityId:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromWorkspace(int workspaceId, int amenityId)
    {
        var removed = await _amenityService.RemoveFromWorkspaceAsync(workspaceId, amenityId);
        if (!removed)
        {
            return NotFound(new { message = $"Amenity assignment for workspace {workspaceId} and amenity {amenityId} was not found." });
        }

        return NoContent();
    }

    /// <summary>
    /// Updates the operational status of an assigned amenity. (Admin restricted)
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace.</param>
    /// <param name="amenityId">The unique identifier of the amenity.</param>
    /// <param name="isOperational">Whether the amenity is operational.</param>
    /// <returns>An OK result on success.</returns>
    /// <response code="200">If the status was successfully updated.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="403">If the caller is not an administrator.</response>
    /// <response code="404">If the assignment was not found.</response>
    [HttpPatch("workspace/{workspaceId:int}/{amenityId:int}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetOperationalStatus(int workspaceId, int amenityId, [FromBody] bool isOperational)
    {
        var updated = await _amenityService.SetOperationalStatusAsync(workspaceId, amenityId, isOperational);
        if (!updated)
        {
            return NotFound(new { message = $"Amenity assignment for workspace {workspaceId} and amenity {amenityId} was not found." });
        }

        return Ok(new { message = $"Amenity operational status updated to {isOperational}." });
    }
}

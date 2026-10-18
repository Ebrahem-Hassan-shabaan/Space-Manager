using SpaceManager.DTOs.Amenities;

namespace SpaceManager.Services.Interfaces;

/// <summary>
/// Defines service operations for managing amenities and their assignment to workspaces.
/// </summary>
public interface IAmenityService
{
    /// <summary>
    /// Retrieves all amenities available in the catalog.
    /// </summary>
    /// <returns>A collection of amenity response DTOs.</returns>
    Task<IEnumerable<AmenityResponseDto>> GetAllAsync();

    /// <summary>
    /// Retrieves an amenity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the amenity.</param>
    /// <returns>The amenity response DTO if found; otherwise, null.</returns>
    Task<AmenityResponseDto?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new amenity in the catalog.
    /// </summary>
    /// <param name="dto">The creation request data.</param>
    /// <returns>The created amenity response DTO.</returns>
    Task<AmenityResponseDto> CreateAsync(CreateAmenityDto dto);

    /// <summary>
    /// Updates an existing amenity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the amenity to update.</param>
    /// <param name="dto">The update request data.</param>
    /// <returns>The updated amenity response DTO if found; otherwise, null.</returns>
    Task<AmenityResponseDto?> UpdateAsync(int id, UpdateAmenityDto dto);

    /// <summary>
    /// Deletes an amenity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the amenity to delete.</param>
    /// <returns>True if the amenity was deleted; false if not found.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the amenity is currently assigned to one or more workspaces.</exception>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Retrieves all amenities associated with a specific workspace, including their operational status.
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace.</param>
    /// <returns>A collection of workspace amenity response DTOs.</returns>
    Task<IEnumerable<WorkspaceAmenityResponseDto>> GetByWorkspaceIdAsync(int workspaceId);

    /// <summary>
    /// Assigns an amenity to a workspace, or updates its operational status if already assigned.
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace.</param>
    /// <param name="dto">The assignment request data.</param>
    /// <returns>True if the amenity was assigned/updated; false if workspace or amenity does not exist.</returns>
    Task<bool> AssignToWorkspaceAsync(int workspaceId, AssignWorkspaceAmenityDto dto);

    /// <summary>
    /// Removes an assigned amenity from a workspace.
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace.</param>
    /// <param name="amenityId">The unique identifier of the amenity to remove.</param>
    /// <returns>True if the assignment was removed; false if not found.</returns>
    Task<bool> RemoveFromWorkspaceAsync(int workspaceId, int amenityId);

    /// <summary>
    /// Updates the operational status of an amenity within a specific workspace.
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace.</param>
    /// <param name="amenityId">The unique identifier of the amenity.</param>
    /// <param name="isOperational">Whether the amenity is operational.</param>
    /// <returns>True if the status was updated; false if the association does not exist.</returns>
    Task<bool> SetOperationalStatusAsync(int workspaceId, int amenityId, bool isOperational);
}

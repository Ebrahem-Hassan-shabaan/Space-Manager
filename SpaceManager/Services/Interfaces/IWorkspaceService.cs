using SpaceManager.DTOs.Workspaces;

namespace SpaceManager.Services.Interfaces;

/// <summary>
/// Defines service operations for managing workspaces.
/// </summary>
public interface IWorkspaceService
{
    /// <summary>
    /// Retrieves all workspaces matching the specified filters.
    /// </summary>
    /// <param name="workspaceTypeId">Optional workspace type ID filter.</param>
    /// <param name="isAvailable">Optional availability status filter.</param>
    /// <returns>A collection of workspace response DTOs.</returns>
    Task<IEnumerable<WorkspaceResponseDto>> GetAllAsync(int? workspaceTypeId = null, bool? isAvailable = null,int? maxCapacity=null);

    /// <summary>
    /// Retrieves a workspace by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the workspace.</param>
    /// <returns>The workspace response DTO if found; otherwise, null.</returns>
    Task<WorkspaceResponseDto?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new workspace from the provided data.
    /// </summary>
    /// <param name="dto">The creation request data.</param>
    /// <returns>The created workspace response DTO.</returns>
    Task<WorkspaceResponseDto> CreateAsync(CreateWorkspaceDto dto);

    /// <summary>
    /// Updates an existing workspace by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the workspace to update.</param>
    /// <param name="dto">The update request data.</param>
    /// <returns>The updated workspace response DTO if found; otherwise, null.</returns>
    Task<WorkspaceResponseDto?> UpdateAsync(int id, UpdateWorkspaceDto dto);

    /// <summary>
    /// Deletes a workspace by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the workspace to delete.</param>
    /// <returns>True if the workspace was deleted; false if not found.</returns>
    /// <exception cref="InvalidOperationException">Thrown if workspace has existing bookings or maintenance tickets.</exception>
    Task<bool> DeleteAsync(int id);
}

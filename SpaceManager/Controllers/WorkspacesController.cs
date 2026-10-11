using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceManager.DTOs.Workspaces;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Controllers;

/// <summary>
/// Provides API endpoints for managing workspaces within the system.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkspacesController"/> class.
    /// </summary>
    /// <param name="workspaceService">The workspace service instance.</param>
    public WorkspacesController(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    /// <summary>
    /// Retrieves a list of workspaces filtered by optional parameters.
    /// Non-admin users are restricted to viewing available workspaces only.
    /// </summary>
    /// <param name="workspaceTypeId">Optional filter by workspace type ID.</param>
    /// <param name="isAvailable">Optional filter for workspace availability (admins only).</param>
    /// <returns>A collection of matching workspace DTOs.</returns>
    /// <response code="200">Returns the matching list of workspaces.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkspaceResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] int? workspaceTypeId = null, [FromQuery] bool? isAvailable = null, [FromQuery] int? maxCapacity = null)
    {
        // Non-admin callers can only view available workspaces
        if (!User.IsInRole("Admin"))
        {
            isAvailable = true;
        }

        var workspaces = await _workspaceService.GetAllAsync(workspaceTypeId, isAvailable, maxCapacity);
        return Ok(workspaces);
    }

    /// <summary>
    /// Retrieves all available workspace types for filtering and selection.
    /// Accessible without authentication to allow public exploration.
    /// </summary>
    /// <returns>A collection of workspace type response DTOs.</returns>
    /// <response code="200">Returns the list of workspace types.</response>
    [HttpGet("types")]
    [ProducesResponseType(typeof(IEnumerable<WorkspaceTypeResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkspaceTypes()
    {
        var types = await _workspaceService.GetWorkspaceTypesAsync();
        return Ok(types);
    }

    /// <summary>
    /// Retrieves details of a specific workspace by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the workspace.</param>
    /// <returns>The workspace details DTO if found.</returns>
    /// <response code="200">Returns the requested workspace details.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="404">If no workspace exists with the specified ID.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WorkspaceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var workspace = await _workspaceService.GetByIdAsync(id);
        if (workspace == null)
        {
            return NotFound();
        }

        return Ok(workspace);
    }

    /// <summary>
    /// Creates a new workspace. (Admin restricted)
    /// </summary>
    /// <param name="dto">The data transfer object containing workspace creation details.</param>
    /// <returns>The newly created workspace DTO with its location header.</returns>
    /// <response code="201">Returns the newly created workspace.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="403">If the user is not in the Admin role.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WorkspaceResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateWorkspaceDto dto)
    {
        var workspace = await _workspaceService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = workspace.Id }, workspace);
    }

    /// <summary>
    /// Updates an existing workspace by its unique identifier. (Admin restricted)
    /// </summary>
    /// <param name="id">The unique identifier of the workspace to update.</param>
    /// <param name="dto">The data transfer object containing updated workspace information.</param>
    /// <returns>The updated workspace DTO if found.</returns>
    /// <response code="200">Returns the updated workspace details.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="403">If the user is not in the Admin role.</response>
    /// <response code="404">If no workspace exists with the specified ID.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WorkspaceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkspaceDto dto)
    {
        var updatedWorkspace = await _workspaceService.UpdateAsync(id, dto);
        if (updatedWorkspace == null)
        {
            return NotFound();
        }

        return Ok(updatedWorkspace);
    }

    /// <summary>
    /// Deletes a workspace by its unique identifier. (Admin restricted)
    /// </summary>
    /// <param name="id">The unique identifier of the workspace to delete.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the workspace was successfully deleted.</response>
    /// <response code="400">If the workspace cannot be deleted due to existing bookings or tickets.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="403">If the user is not in the Admin role.</response>
    /// <response code="404">If no workspace exists with the specified ID.</response>
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
            var result = await _workspaceService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

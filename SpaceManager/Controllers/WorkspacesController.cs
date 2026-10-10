using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceManager.DTOs.Workspaces;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;

    public WorkspacesController(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? workspaceTypeId = null, [FromQuery] bool? isAvailable = null)
    {
        // Non-admin callers can only view available workspaces
        if (!User.IsInRole("Admin"))
        {
            isAvailable = true;
        }

        var workspaces = await _workspaceService.GetAllAsync(workspaceTypeId, isAvailable);
        return Ok(workspaces);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var workspace = await _workspaceService.GetByIdAsync(id);
        if (workspace == null)
        {
            return NotFound();
        }

        return Ok(workspace);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateWorkspaceDto dto)
    {
        var workspace = await _workspaceService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = workspace.Id }, workspace);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkspaceDto dto)
    {
        var updatedWorkspace = await _workspaceService.UpdateAsync(id, dto);
        if (updatedWorkspace == null)
        {
            return NotFound();
        }

        return Ok(updatedWorkspace);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
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

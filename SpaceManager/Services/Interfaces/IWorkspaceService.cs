using SpaceManager.DTOs.Workspaces;
namespace SpaceManager.Services.Interfaces;

public interface IWorkspaceService
{
    Task<IEnumerable<WorkspaceResponseDto>> GetAllAsync(int? workspaceTypeId = null, bool? isAvailable = null);
    Task<WorkspaceResponseDto?> GetByIdAsync(int id);
    Task<WorkspaceResponseDto> CreateAsync(CreateWorkspaceDto dto);
    Task<WorkspaceResponseDto?> UpdateAsync(int id, UpdateWorkspaceDto dto);
    Task<bool> DeleteAsync(int id);
}

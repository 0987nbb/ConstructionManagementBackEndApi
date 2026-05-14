using ConstructionManagement.Dtos;

namespace ConstructionManagement.BLL.Services;

public interface IProjectService
{
    Task<ApiResponseDto<ProjectDto>> CreateAsync(CreateProjectDto dto);
    Task<ApiResponseDto<ProjectDto>> UpdateAsync(Guid id, UpdateProjectDto dto);
    Task<ApiResponseDto<bool>> DeleteAsync(Guid id);
    Task<ApiResponseDto<ProjectDto>> UpdateStatusAsync(Guid id, UpdateProjectStatusDto dto);
    Task<ApiResponseDto<ProjectDto>> UpdateProgressAsync(Guid id, UpdateProjectProgressDto dto);

    Task<ApiResponseDto<List<ProjectDto>>> GetAllForUserAsync(ProjectQueryDto query, Guid userId, string role, string? email);
    Task<ApiResponseDto<ProjectDto>> GetByIdForUserAsync(Guid id, Guid userId, string role, string? email);

    Task<ApiResponseDto<List<ProjectFinancialDto>>> GetFinancialForUserAsync(ProjectQueryDto query, Guid userId, string role, string? email);
    Task<ApiResponseDto<ProjectFinancialDto>> GetFinancialByIdForUserAsync(Guid id, Guid userId, string role, string? email);
}

using ConstructionManagement.Dtos;

namespace ConstructionManagement.BLL.Services;

public interface ISiteService
{
    Task<ApiResponseDto<SiteDto>> CreateAsync(CreateSiteDto dto);
    Task<ApiResponseDto<SiteDto>> UpdateAsync(Guid id, UpdateSiteDto dto);
    Task<ApiResponseDto<bool>> DeleteAsync(Guid id);
    Task<ApiResponseDto<SiteDto>> AssignEngineerAsync(Guid id, AssignSiteEngineerDto dto);
    Task<ApiResponseDto<SiteDto>> UpdateStatusForUserAsync(Guid id, UpdateSiteStatusDto dto, Guid userId, string role, string? email);
    Task<ApiResponseDto<SiteDto>> UpdateProgressForUserAsync(Guid id, UpdateSiteProgressDto dto, Guid userId, string role, string? email);
    Task<ApiResponseDto<List<SiteDto>>> GetAllForUserAsync(SiteQueryDto query, Guid userId, string role, string? email);
    Task<ApiResponseDto<SiteDto>> GetByIdForUserAsync(Guid id, Guid userId, string role, string? email);
}

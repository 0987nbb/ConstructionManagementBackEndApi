using ConstructionManagement.Dtos;

namespace ConstructionManagement.BLL.Services;

public interface IDashboardService
{
    Task<ApiResponseDto<DashboardKpiDto>> GetKpisAsync();
}

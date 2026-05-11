using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Dtos;

namespace ConstructionManagement.BLL.Services;

public class DashboardService : IDashboardService
{
    private readonly IUserRepository _userRepository;

    public DashboardService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ApiResponseDto<DashboardKpiDto>> GetKpisAsync()
    {
        var active = await _userRepository.CountActiveUsersAsync();
        var inactive = await _userRepository.CountInactiveUsersAsync();

        var result = new DashboardKpiDto
        {
            ActiveUsers = active,
            InactiveUsers = inactive,
            TotalUsers = active + inactive,
            AdminCount = await _userRepository.CountByRoleAsync(ApplicationRoles.Admin),
            ProjectManagerCount = await _userRepository.CountByRoleAsync(ApplicationRoles.ProjectManager),
            EngineerCount = await _userRepository.CountByRoleAsync(ApplicationRoles.Engineer),
            AccountantCount = await _userRepository.CountByRoleAsync(ApplicationRoles.Accountant),
            ClientCount = await _userRepository.CountByRoleAsync(ApplicationRoles.Client)
        };

        return ApiResponseDto<DashboardKpiDto>.Ok(result);
    }
}

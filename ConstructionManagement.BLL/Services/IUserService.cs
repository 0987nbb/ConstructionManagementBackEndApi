using ConstructionManagement.Dtos;

namespace ConstructionManagement.BLL.Services;

public interface IUserService
{
    Task<ApiResponseDto<CreateStaffUserResponseDto>> AddUserAsync(CreateUserDto dto);
    Task<ApiResponseDto<List<UserDto>>> GetAllUsersAsync(UserQueryDto query);
    Task<ApiResponseDto<UserDto>> GetUserByIdAsync(Guid id);
    Task<ApiResponseDto<UserDto>> UpdateUserAsync(Guid id, UpdateUserDto dto);
    Task<ApiResponseDto<bool>> DeleteUserAsync(Guid id);
    Task<ApiResponseDto<UserDto>> UpdateUserStatusAsync(Guid id, UpdateUserStatusDto dto);
    Task<ApiResponseDto<UserDto>> AssignRoleAsync(Guid id, AssignRoleDto dto);
    Task<ApiResponseDto<UserDto>> GetProfileAsync(Guid userId);
    Task<ApiResponseDto<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
}

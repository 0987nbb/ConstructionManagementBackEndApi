using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Domain.Entities;
using ConstructionManagement.Dtos;

namespace ConstructionManagement.BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ApiResponseDto<UserDto>> AddUserAsync(CreateUserDto dto)
    {
        if (!ApplicationRoles.All.Contains(dto.Role))
        {
            return ApiResponseDto<UserDto>.Fail("Invalid role provided.");
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var exists = await _userRepository.EmailExistsAsync(normalizedEmail);
        if (exists)
        {
            return ApiResponseDto<UserDto>.Fail("A user with this email already exists.");
        }

        var entity = new AppUser
        {
            FullName = dto.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12),
            Role = dto.Role.Trim(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            IsActive = dto.IsActive,
            IsDeleted = false
        };

        await _userRepository.AddAsync(entity);
        await _userRepository.SaveChangesAsync();

        return ApiResponseDto<UserDto>.Ok(Map(entity), "User created successfully.");
    }

    public async Task<ApiResponseDto<List<UserDto>>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllActiveAsync();
        return ApiResponseDto<List<UserDto>>.Ok(users.Select(Map).ToList());
    }

    public async Task<ApiResponseDto<UserDto>> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdActiveAsync(id);
        if (user == null)
        {
            return ApiResponseDto<UserDto>.Fail("User not found.");
        }

        return ApiResponseDto<UserDto>.Ok(Map(user));
    }

    public async Task<ApiResponseDto<UserDto>> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdActiveAsync(id);
        if (user == null)
        {
            return ApiResponseDto<UserDto>.Fail("User not found.");
        }

        user.FullName = dto.FullName.Trim();
        user.PhoneNumber = dto.PhoneNumber?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync();
        return ApiResponseDto<UserDto>.Ok(Map(user), "User updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdActiveAsync(id);
        if (user == null)
        {
            return ApiResponseDto<bool>.Fail("User not found.");
        }

        user.IsDeleted = true;
        user.IsActive = false;
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync();
        return ApiResponseDto<bool>.Ok(true, "User deleted successfully.");
    }

    public async Task<ApiResponseDto<UserDto>> UpdateUserStatusAsync(Guid id, UpdateUserStatusDto dto)
    {
        var user = await _userRepository.GetByIdActiveAsync(id);
        if (user == null)
        {
            return ApiResponseDto<UserDto>.Fail("User not found.");
        }

        user.IsActive = dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync();
        return ApiResponseDto<UserDto>.Ok(Map(user), dto.IsActive ? "User activated successfully." : "User deactivated successfully.");
    }

    public async Task<ApiResponseDto<UserDto>> AssignRoleAsync(Guid id, AssignRoleDto dto)
    {
        if (!ApplicationRoles.All.Contains(dto.Role))
        {
            return ApiResponseDto<UserDto>.Fail("Invalid role provided.");
        }

        var user = await _userRepository.GetByIdActiveAsync(id);
        if (user == null)
        {
            return ApiResponseDto<UserDto>.Fail("User not found.");
        }

        user.Role = dto.Role.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync();
        return ApiResponseDto<UserDto>.Ok(Map(user), "User role updated successfully.");
    }

    public async Task<ApiResponseDto<UserDto>> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdActiveAsync(userId);
        if (user == null)
        {
            return ApiResponseDto<UserDto>.Fail("User not found.");
        }

        return ApiResponseDto<UserDto>.Ok(Map(user));
    }

    public async Task<ApiResponseDto<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _userRepository.GetByIdActiveAsync(userId);
        if (user == null)
        {
            return ApiResponseDto<UserDto>.Fail("User not found.");
        }

        user.FullName = dto.FullName.Trim();
        user.PhoneNumber = dto.PhoneNumber?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync();
        return ApiResponseDto<UserDto>.Ok(Map(user), "Profile updated successfully.");
    }

    private static UserDto Map(AppUser user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role,
        PhoneNumber = user.PhoneNumber,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}

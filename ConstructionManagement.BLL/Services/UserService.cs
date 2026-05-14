using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Domain.Entities;
using ConstructionManagement.Dtos;

namespace ConstructionManagement.BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;

    public UserService(IUserRepository userRepository, IAuditService auditService)
    {
        _userRepository = userRepository;
        _auditService = auditService;
    }

    public async Task<ApiResponseDto<CreateStaffUserResponseDto>> AddUserAsync(CreateUserDto dto)
    {
        var normalizedRole = ApplicationRoles.Normalize(dto.Role);
        if (normalizedRole == null)
        {
            return ApiResponseDto<CreateStaffUserResponseDto>.Fail("Invalid role provided.");
        }

        if (!ApplicationRoles.IsStaffAssignableRole(normalizedRole))
        {
            return ApiResponseDto<CreateStaffUserResponseDto>.Fail(
                "Staff accounts can only be created as Project Manager, Engineer, or Accountant. Clients register themselves; Admin is seeded.");
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var exists = await _userRepository.EmailExistsAsync(normalizedEmail);
        if (exists)
        {
            return ApiResponseDto<CreateStaffUserResponseDto>.Fail("A user with this email already exists.");
        }

        var tempPassword = dto.TemporaryPassword.Trim();
        var entity = new AppUser
        {
            FullName = dto.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword, workFactor: 12),
            Role = normalizedRole,
            PhoneNumber = dto.PhoneNumber?.Trim(),
            IsActive = dto.IsActive,
            IsDeleted = false,
            MustChangePassword = false,
            IsFirstLogin = true,
            PasswordSetupTokenHash = null,
            PasswordSetupTokenExpiresAtUtc = null,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(entity);
        await _userRepository.SaveChangesAsync();

        return ApiResponseDto<CreateStaffUserResponseDto>.Ok(new CreateStaffUserResponseDto
        {
            User = Map(entity),
            TemporaryPassword = tempPassword
        }, "Staff user created with temporary credentials. Share credentials securely.");
    }

    public async Task<ApiResponseDto<List<UserDto>>> GetAllUsersAsync(UserQueryDto query)
    {
        string? normalizedRole = null;
        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            normalizedRole = ApplicationRoles.Normalize(query.Role);
            if (normalizedRole == null)
            {
                return ApiResponseDto<List<UserDto>>.Fail("Invalid role filter.");
            }
        }

        var users = await _userRepository.SearchAsync(query.Search, normalizedRole, query.IsActive);
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
        var normalizedRole = ApplicationRoles.Normalize(dto.Role);
        if (normalizedRole == null)
        {
            return ApiResponseDto<UserDto>.Fail("Invalid role provided.");
        }

        if (normalizedRole == ApplicationRoles.Admin || normalizedRole == ApplicationRoles.Client)
        {
            return ApiResponseDto<UserDto>.Fail("Admin and Client roles cannot be assigned from user management.");
        }

        var user = await _userRepository.GetByIdActiveAsync(id);
        if (user == null)
        {
            return ApiResponseDto<UserDto>.Fail("User not found.");
        }

        user.Role = normalizedRole;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync();
        await _auditService.LogAsync(user.Id, "user.role.changed", null, new { user.Id, Role = user.Role });
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

    public async Task<ApiResponseDto<AdminPasswordResetResponseDto>> AdminResetTemporaryPasswordAsync(Guid id, AdminResetPasswordDto dto)
    {
        var user = await _userRepository.GetByIdActiveAsync(id);
        if (user == null)
        {
            return ApiResponseDto<AdminPasswordResetResponseDto>.Fail("User not found.");
        }

        var temp = dto.TemporaryPassword.Trim();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(temp, workFactor: 12);
        user.IsFirstLogin = true;
        user.MustChangePassword = false;
        user.PasswordSetupTokenHash = null;
        user.PasswordSetupTokenExpiresAtUtc = null;
        user.FailedLoginAttempts = 0;
        user.LockoutEndUtc = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync();
        await _auditService.LogAsync(user.Id, "user.password.reset_by_admin", null, null);

        return ApiResponseDto<AdminPasswordResetResponseDto>.Ok(
            new AdminPasswordResetResponseDto { TemporaryPassword = temp },
            "Temporary password issued. Require the user to sign in with it and finish first-login onboarding.");
    }

    private static UserDto Map(AppUser user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role,
        PhoneNumber = user.PhoneNumber,
        IsActive = user.IsActive,
        MustChangePassword = user.MustChangePassword,
        IsFirstLogin = user.IsFirstLogin,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}

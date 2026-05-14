using ConstructionManagement.Dtos;

namespace ConstructionManagement.BLL.Services
{
public interface IAuthService
{
    Task<AuthResultDto> Register(RegisterDto dto);
    Task<AuthResultDto> Login(LoginDto dto, string? ipAddress);
    Task<AuthResultDto> SetPassword(SetPasswordDto dto);
    Task<AuthResultDto> CompleteFirstLogin(Guid userId, CompleteFirstLoginDto dto);
    Task<AuthResultDto> RefreshTokenAsync(string refreshToken, string? ipAddress);
    Task<ApiResponseDto<bool>> LogoutAsync(Guid userId, string refreshToken, string? ipAddress);
    Task<ApiResponseDto<bool>> RequestPasswordResetAsync(RequestPasswordResetDto dto, string? ipAddress);
    Task<ApiResponseDto<bool>> ResetPasswordAsync(ResetPasswordDto dto, string? ipAddress);
}
}

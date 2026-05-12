using ConstructionManagement.Dtos;

namespace ConstructionManagement.BLL.Services
{
public interface IAuthService
{
    Task<AuthResultDto> Register(RegisterDto dto);
    Task<AuthResultDto> Login(LoginDto dto);
    Task<AuthResultDto> SetPassword(SetPasswordDto dto);
    Task<AuthResultDto> CompleteFirstLogin(Guid userId, CompleteFirstLoginDto dto);
}
}

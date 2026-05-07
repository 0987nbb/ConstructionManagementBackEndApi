using ConstructionManagement.Dtos;

namespace ConstructionManagement.BLL.Services
{
    public interface IAuthService
    {
        Task<string> Register(RegisterDto dto);
        Task<string> Login(LoginDto dto);
    }
}

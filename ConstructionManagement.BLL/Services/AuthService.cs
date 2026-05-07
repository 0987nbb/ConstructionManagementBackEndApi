using ConstructionManagement.Domain.Entities;
using ConstructionManagement.Dtos;
using ConstructionManagement.DAL.Data;
using Microsoft.EntityFrameworkCore;
namespace ConstructionManagement.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        public AuthService(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<string> Register(RegisterDto dto)
        {
            var user = new AppUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Client"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return "User Registered";
        }

        public async Task<string> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
                return "Invalid Email";

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return "Invalid Password";

            return _tokenService.CreateToken(user);
        }
    }
}

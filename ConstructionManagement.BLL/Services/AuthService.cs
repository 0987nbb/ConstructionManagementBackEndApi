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

        public async Task<AuthResultDto> Register(RegisterDto dto)
        {
            var exists = await _context.Users.AnyAsync(x => x.Email == dto.Email);
            if (exists)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Email already exists"
                };
            }

            var user = new AppUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Client"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResultDto
            {
                Success = true,
                Message = "User Registered"
            };
        }

        public async Task<AuthResultDto> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Invalid Email"
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Invalid Password"
                };
            }

            return new AuthResultDto
            {
                Success = true,
                Message = "Login Successful",
                Token = _tokenService.CreateToken(user)
            };
        }
    }
}

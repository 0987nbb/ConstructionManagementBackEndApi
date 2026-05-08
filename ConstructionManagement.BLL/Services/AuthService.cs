using ConstructionManagement.DAL.Data;
using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Domain.Entities;
using ConstructionManagement.Dtos;
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
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var exists = await _context.Users.AnyAsync(x => x.Email == normalizedEmail);
            if (exists)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "An account with this email already exists."
                };
            }

            var user = new AppUser
            {
                FullName = dto.FullName.Trim(),
                Email = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12),
                Role = ApplicationRoles.Client
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResultDto
            {
                Success = true,
                Message = "User registered successfully.",
                Role = user.Role
            };
        }

        public async Task<AuthResultDto> Login(LoginDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            if (!ApplicationRoles.All.Contains(user.Role))
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "User role is invalid. Contact administrator."
                };
            }

            var tokenResult = _tokenService.CreateToken(user);
            return new AuthResultDto
            {
                Success = true,
                Message = "Login successful.",
                Token = tokenResult.Token,
                Role = user.Role,
                ExpiresAtUtc = tokenResult.ExpiresAtUtc
            };
        }
    }
}

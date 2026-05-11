using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Domain.Entities;
using ConstructionManagement.Dtos;
using System.Security.Cryptography;
using System.Text;

namespace ConstructionManagement.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly TokenService _tokenService;

        public AuthService(IUserRepository userRepository, TokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<AuthResultDto> Register(RegisterDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var exists = await _userRepository.EmailExistsAsync(normalizedEmail);
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
                Role = ApplicationRoles.Client,
                IsActive = true,
                IsDeleted = false,
                MustChangePassword = false
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

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
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);
            if (user == null || user.IsDeleted || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            if (user.MustChangePassword)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Password setup is pending. Use your invite link to set password first."
                };
            }

            if (!user.IsActive)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "User account is inactive. Contact administrator."
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

        public async Task<AuthResultDto> SetPassword(SetPasswordDto dto)
        {
            var tokenHash = HashToken(dto.Token.Trim());
            var user = await _userRepository.GetByPasswordSetupTokenHashAsync(tokenHash);
            if (user == null || user.PasswordSetupTokenExpiresAtUtc is null || user.PasswordSetupTokenExpiresAtUtc < DateTime.UtcNow)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Invite link is invalid or expired."
                };
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);
            user.MustChangePassword = false;
            user.PasswordSetupTokenHash = null;
            user.PasswordSetupTokenExpiresAtUtc = null;
            user.UpdatedAt = DateTime.UtcNow;
            user.IsActive = true;

            await _userRepository.SaveChangesAsync();

            return new AuthResultDto
            {
                Success = true,
                Message = "Password set successfully. You can now log in.",
                Role = user.Role
            };
        }

        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }
    }
}

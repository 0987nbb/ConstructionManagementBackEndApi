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
                MustChangePassword = false,
                IsFirstLogin = false
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
                    Message = "Password setup is pending. Complete the emailed invite link before signing in.",
                    IsFirstLogin = false
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

            var normalizedRole = ApplicationRoles.Normalize(user.Role);
            if (normalizedRole == null)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "User role is invalid. Contact administrator."
                };
            }

            user.Role = normalizedRole;

            var tokenResult = _tokenService.CreateToken(user);
            return new AuthResultDto
            {
                Success = true,
                Message = "Login successful.",
                Token = tokenResult.Token,
                Role = user.Role,
                ExpiresAtUtc = tokenResult.ExpiresAtUtc,
                IsFirstLogin = user.IsFirstLogin
            };
        }

        public async Task<AuthResultDto> CompleteFirstLogin(Guid userId, CompleteFirstLoginDto dto)
        {
            var user = await _userRepository.GetByIdActiveAsync(userId);
            if (user == null)
            {
                return new AuthResultDto { Success = false, Message = "User not found." };
            }

            if (!user.IsFirstLogin)
            {
                return new AuthResultDto { Success = false, Message = "First login onboarding is already complete." };
            }

            if (user.Role == ApplicationRoles.Client)
            {
                return new AuthResultDto { Success = false, Message = "This flow is only for internal staff accounts." };
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                return new AuthResultDto { Success = false, Message = "Current password is incorrect." };
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 12);
            user.FullName = dto.FullName.Trim();
            user.PhoneNumber = dto.PhoneNumber.Trim();
            user.IsFirstLogin = false;
            user.UpdatedAt = DateTime.UtcNow;

            var persistedRole = ApplicationRoles.Normalize(user.Role);
            if (persistedRole != null)
            {
                user.Role = persistedRole;
            }

            await _userRepository.SaveChangesAsync();

            var tokenResult = _tokenService.CreateToken(user);
            return new AuthResultDto
            {
                Success = true,
                Message = "Profile and password saved. Welcome.",
                Token = tokenResult.Token,
                Role = user.Role,
                ExpiresAtUtc = tokenResult.ExpiresAtUtc,
                IsFirstLogin = false
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
            user.IsFirstLogin = false;
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

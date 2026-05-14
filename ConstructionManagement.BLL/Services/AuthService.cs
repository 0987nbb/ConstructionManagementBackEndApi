using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Domain.Entities;
using ConstructionManagement.Dtos;

namespace ConstructionManagement.BLL.Services
{
    public class AuthService : IAuthService
    {
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan PasswordResetTokenLifetime = TimeSpan.FromHours(1);

        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly TokenService _tokenService;
        private readonly IAuditService _auditService;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            TokenService tokenService,
            IAuditService auditService)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _tokenService = tokenService;
            _auditService = auditService;
        }

        public async Task<AuthResultDto> Register(RegisterDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var exists = await _userRepository.EmailExistsAsync(normalizedEmail);
            if (exists)
            {
                return new AuthResultDto { Success = false, Message = "An account with this email already exists." };
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

            return new AuthResultDto { Success = true, Message = "User registered successfully.", Role = user.Role };
        }

        public async Task<AuthResultDto> Login(LoginDto dto, string? ipAddress)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            if (user == null || user.IsDeleted)
            {
                await _auditService.LogAsync(null, "login.failed", ipAddress, new { Email = normalizedEmail, Reason = "invalid_credentials" });
                return new AuthResultDto { Success = false, Message = "Invalid email or password." };
            }

            if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > DateTime.UtcNow)
            {
                await _auditService.LogAsync(user.Id, "login.locked_out", ipAddress, new { user.Email, user.LockoutEndUtc });
                return new AuthResultDto { Success = false, Message = "Account is locked. Try again later." };
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                user.FailedLoginAttempts += 1;
                user.LastFailedLoginAtUtc = DateTime.UtcNow;
                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockoutEndUtc = DateTime.UtcNow.Add(LockoutDuration);
                    user.FailedLoginAttempts = 0;
                }

                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();
                await _auditService.LogAsync(user.Id, "login.failed", ipAddress, new { user.Email, user.LockoutEndUtc });
                return new AuthResultDto { Success = false, Message = "Invalid email or password." };
            }

            if (!user.IsActive)
            {
                await _auditService.LogAsync(user.Id, "login.blocked_inactive", ipAddress, null);
                return new AuthResultDto { Success = false, Message = "User account is inactive. Contact administrator." };
            }

            var normalizedRole = ApplicationRoles.Normalize(user.Role);
            if (normalizedRole == null)
            {
                return new AuthResultDto { Success = false, Message = "User role is invalid. Contact administrator." };
            }

            user.Role = normalizedRole;
            user.FailedLoginAttempts = 0;
            user.LockoutEndUtc = null;
            user.LastFailedLoginAtUtc = null;
            user.UpdatedAt = DateTime.UtcNow;

            var access = _tokenService.CreateAccessToken(user);
            var refresh = _tokenService.CreateRefreshToken();
            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refresh.TokenHash,
                ExpiresAtUtc = refresh.ExpiresAtUtc,
                CreatedByIp = ipAddress
            });
            await _userRepository.SaveChangesAsync();

            await _auditService.LogAsync(user.Id, "login.success", ipAddress, null);

            return new AuthResultDto
            {
                Success = true,
                Message = "Login successful.",
                Token = access.Token,
                RefreshToken = refresh.RawToken,
                Role = user.Role,
                ExpiresAtUtc = access.ExpiresAtUtc,
                IsFirstLogin = user.IsFirstLogin
            };
        }

        public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken, string? ipAddress)
        {
            var tokenHash = TokenService.HashToken(refreshToken.Trim());
            var stored = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);
            if (stored == null || stored.User == null || stored.User.IsDeleted || !stored.User.IsActive || stored.IsRevoked || stored.ExpiresAtUtc <= DateTime.UtcNow)
            {
                return new AuthResultDto { Success = false, Message = "Invalid refresh token." };
            }

            var user = stored.User;
            var normalizedRole = ApplicationRoles.Normalize(user.Role);
            if (normalizedRole == null)
            {
                return new AuthResultDto { Success = false, Message = "User role is invalid. Contact administrator." };
            }

            user.Role = normalizedRole;

            var newAccess = _tokenService.CreateAccessToken(user);
            var newRefresh = _tokenService.CreateRefreshToken();

            stored.IsRevoked = true;
            stored.RevokedAtUtc = DateTime.UtcNow;
            stored.ReplacedByTokenHash = newRefresh.TokenHash;

            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = newRefresh.TokenHash,
                ExpiresAtUtc = newRefresh.ExpiresAtUtc,
                CreatedByIp = ipAddress
            });

            await _refreshTokenRepository.SaveChangesAsync();

            return new AuthResultDto
            {
                Success = true,
                Message = "Token refreshed successfully.",
                Token = newAccess.Token,
                RefreshToken = newRefresh.RawToken,
                Role = user.Role,
                ExpiresAtUtc = newAccess.ExpiresAtUtc,
                IsFirstLogin = user.IsFirstLogin
            };
        }

        public async Task<ApiResponseDto<bool>> LogoutAsync(Guid userId, string refreshToken, string? ipAddress)
        {
            var tokenHash = TokenService.HashToken(refreshToken.Trim());
            var stored = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);
            if (stored == null || stored.UserId != userId)
            {
                return ApiResponseDto<bool>.Fail("Invalid refresh token.");
            }

            if (!stored.IsRevoked)
            {
                stored.IsRevoked = true;
                stored.RevokedAtUtc = DateTime.UtcNow;
                await _refreshTokenRepository.SaveChangesAsync();
            }

            await _auditService.LogAsync(userId, "logout.success", ipAddress, null);
            return ApiResponseDto<bool>.Ok(true, "Logged out successfully.");
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
            user.MustChangePassword = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _refreshTokenRepository.RevokeAllActiveForUserAsync(user.Id, DateTime.UtcNow);
            await _userRepository.SaveChangesAsync();

            var access = _tokenService.CreateAccessToken(user);
            var refresh = _tokenService.CreateRefreshToken();
            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refresh.TokenHash,
                ExpiresAtUtc = refresh.ExpiresAtUtc
            });
            await _refreshTokenRepository.SaveChangesAsync();

            return new AuthResultDto
            {
                Success = true,
                Message = "Profile and password saved. Welcome.",
                Token = access.Token,
                RefreshToken = refresh.RawToken,
                Role = user.Role,
                ExpiresAtUtc = access.ExpiresAtUtc,
                IsFirstLogin = false
            };
        }

        public async Task<AuthResultDto> SetPassword(SetPasswordDto dto)
        {
            var tokenHash = TokenService.HashToken(dto.Token.Trim());
            var user = await _userRepository.GetByPasswordSetupTokenHashAsync(tokenHash);
            if (user == null || user.PasswordSetupTokenExpiresAtUtc is null || user.PasswordSetupTokenExpiresAtUtc < DateTime.UtcNow)
            {
                return new AuthResultDto { Success = false, Message = "Invite link is invalid or expired." };
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);
            user.MustChangePassword = false;
            user.IsFirstLogin = false;
            user.PasswordSetupTokenHash = null;
            user.PasswordSetupTokenExpiresAtUtc = null;
            user.UpdatedAt = DateTime.UtcNow;
            user.IsActive = true;
            await _refreshTokenRepository.RevokeAllActiveForUserAsync(user.Id, DateTime.UtcNow);
            await _userRepository.SaveChangesAsync();

            return new AuthResultDto { Success = true, Message = "Password set successfully. You can now log in.", Role = user.Role };
        }

        public async Task<ApiResponseDto<bool>> RequestPasswordResetAsync(RequestPasswordResetDto dto, string? ipAddress)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            if (user != null && !user.IsDeleted)
            {
                await _passwordResetTokenRepository.RevokeAllActiveForUserAsync(user.Id, DateTime.UtcNow);
                var refresh = _tokenService.CreateRefreshToken();
                await _passwordResetTokenRepository.AddAsync(new PasswordResetToken
                {
                    UserId = user.Id,
                    TokenHash = refresh.TokenHash,
                    ExpiresAtUtc = DateTime.UtcNow.Add(PasswordResetTokenLifetime)
                });
                await _passwordResetTokenRepository.SaveChangesAsync();

                await _auditService.LogAsync(user.Id, "password_reset.requested", ipAddress, new { user.Email });
            }

            return ApiResponseDto<bool>.Ok(true, "If the account exists, a password reset token has been issued.");
        }

        public async Task<ApiResponseDto<bool>> ResetPasswordAsync(ResetPasswordDto dto, string? ipAddress)
        {
            var tokenHash = TokenService.HashToken(dto.Token.Trim());
            var token = await _passwordResetTokenRepository.GetValidByTokenHashAsync(tokenHash, DateTime.UtcNow);
            if (token == null || token.User == null || token.User.IsDeleted)
            {
                return ApiResponseDto<bool>.Fail("Reset token is invalid or expired.");
            }

            token.UsedAtUtc = DateTime.UtcNow;
            token.IsRevoked = true;

            var user = token.User;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 12);
            user.MustChangePassword = false;
            user.IsFirstLogin = false;
            user.FailedLoginAttempts = 0;
            user.LockoutEndUtc = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _refreshTokenRepository.RevokeAllActiveForUserAsync(user.Id, DateTime.UtcNow);
            await _passwordResetTokenRepository.RevokeAllActiveForUserAsync(user.Id, DateTime.UtcNow);
            await _passwordResetTokenRepository.SaveChangesAsync();

            await _auditService.LogAsync(user.Id, "password_reset.completed", ipAddress, null);

            return ApiResponseDto<bool>.Ok(true, "Password has been reset successfully.");
        }
    }
}

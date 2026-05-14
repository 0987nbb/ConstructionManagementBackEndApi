using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ConstructionManagement.BLL.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(AppUser user)
        {
            var jwtKey = GetJwtKey();
            var jwtIssuer = _config["JWT_ISSUER"] ?? _config["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT issuer is missing.");
            var jwtAudience = _config["JWT_AUDIENCE"] ?? _config["Jwt:Audience"] ?? throw new InvalidOperationException("JWT audience is missing.");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtCustomClaims.IsFirstLogin, user.IsFirstLogin ? "true" : "false")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(30);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
        }

        public (string RawToken, string TokenHash, DateTime ExpiresAtUtc) CreateRefreshToken()
        {
            var rawBytes = RandomNumberGenerator.GetBytes(64);
            var rawToken = Convert.ToBase64String(rawBytes);
            var tokenHash = HashToken(rawToken);
            var expiresAtUtc = DateTime.UtcNow.AddDays(7);
            return (rawToken, tokenHash, expiresAtUtc);
        }

        public static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }

        public string GetJwtKey()
        {
            var jwtKey = _config["JWT_KEY"] ?? _config["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException("JWT key is missing. Set JWT_KEY or Jwt:Key.");
            }

            if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
            {
                throw new InvalidOperationException("JWT key must be at least 32 bytes for HS256.");
            }

            return jwtKey;
        }
    }
}

using ConstructionManagement.Domain.Entities;

namespace ConstructionManagement.DAL.Repositories.Interfaces;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task RevokeAllActiveForUserAsync(Guid userId, DateTime revokedAtUtc, string? replacedByHash = null);
}

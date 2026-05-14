using ConstructionManagement.Domain.Entities;

namespace ConstructionManagement.DAL.Repositories.Interfaces;

public interface IPasswordResetTokenRepository : IGenericRepository<PasswordResetToken>
{
    Task<PasswordResetToken?> GetValidByTokenHashAsync(string tokenHash, DateTime nowUtc);
    Task RevokeAllActiveForUserAsync(Guid userId, DateTime nowUtc);
}

using ConstructionManagement.Domain.Entities;

namespace ConstructionManagement.DAL.Repositories.Interfaces;

public interface IUserRepository
    : IGenericRepository<AppUser>
{
    Task<bool> EmailExistsAsync(string normalizedEmail);
    Task<AppUser?> GetByEmailAsync(string normalizedEmail);
    Task<AppUser?> GetByPasswordSetupTokenHashAsync(string tokenHash);
    Task<List<AppUser>> GetAllActiveAsync();
    Task<AppUser?> GetByIdActiveAsync(Guid id);
    Task<AppUser?> GetByIdIncludingDeletedAsync(Guid id);
    Task<int> CountActiveUsersAsync();
    Task<int> CountInactiveUsersAsync();
    Task<int> CountByRoleAsync(string role);
}

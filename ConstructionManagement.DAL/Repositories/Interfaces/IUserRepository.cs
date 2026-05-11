using ConstructionManagement.Domain.Entities;

namespace ConstructionManagement.DAL.Repositories.Interfaces;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string normalizedEmail);
    Task AddAsync(AppUser user);
    Task<List<AppUser>> GetAllActiveAsync();
    Task<AppUser?> GetByIdActiveAsync(Guid id);
    Task<AppUser?> GetByIdIncludingDeletedAsync(Guid id);
    Task<int> CountActiveUsersAsync();
    Task<int> CountInactiveUsersAsync();
    Task<int> CountByRoleAsync(string role);
    Task SaveChangesAsync();
}

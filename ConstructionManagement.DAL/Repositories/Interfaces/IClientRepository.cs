using ConstructionManagement.Domain.Entities;

namespace ConstructionManagement.DAL.Repositories.Interfaces;

public interface IClientRepository : IGenericRepository<Client>
{
    Task<bool> EmailExistsAsync(string normalizedEmail, Guid? excludingId = null);
    Task<Client?> GetActiveByIdAsync(Guid id);
    Task<List<Client>> GetAllActiveAsync(string? search, bool? isActive);
}

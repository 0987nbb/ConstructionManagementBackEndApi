using ConstructionManagement.Domain.Entities;

namespace ConstructionManagement.DAL.Repositories.Interfaces;

public interface ISiteRepository : IGenericRepository<Site>
{
    Task<Site?> GetByIdWithDetailsAsync(Guid id);
    Task<List<Site>> SearchActiveAsync(string? search, string? status, Guid? projectId);
}

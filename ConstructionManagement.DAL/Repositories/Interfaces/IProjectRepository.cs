using ConstructionManagement.Domain.Entities;

namespace ConstructionManagement.DAL.Repositories.Interfaces;

public interface IProjectRepository : IGenericRepository<Project>
{
    Task<Project?> GetByIdWithDetailsAsync(Guid id);
    Task<List<Project>> SearchActiveAsync(string? search, string? status, Guid? clientId);
}

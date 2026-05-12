using ConstructionManagement.Domain.Entities;

namespace ConstructionManagement.DAL.Repositories.Interfaces;

public interface IProjectRepository : IGenericRepository<Project>
{
    Task<Project?> GetByCodeAsync(string code);
}

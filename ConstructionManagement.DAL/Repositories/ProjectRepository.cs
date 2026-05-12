using ConstructionManagement.DAL.Data;
using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.DAL.Repositories;

public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<Project?> GetByCodeAsync(string code) =>
        _context.Projects.FirstOrDefaultAsync(x => x.Code == code);
}

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

    public Task<Project?> GetByIdWithDetailsAsync(Guid id) =>
        _context.Projects
            .Include(x => x.Client)
            .Include(x => x.AssignedEngineer)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && !x.Client!.IsDeleted);

    public Task<List<Project>> SearchActiveAsync(string? search, string? status, Guid? clientId)
    {
        var query = _context.Projects
            .Include(x => x.Client)
            .Include(x => x.AssignedEngineer)
            .Where(x => !x.IsDeleted && !x.Client!.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(x =>
                x.ProjectName.ToLower().Contains(s) ||
                (x.Description != null && x.Description.ToLower().Contains(s)) ||
                x.Client!.Name.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim();
            query = query.Where(x => x.Status == normalized);
        }

        if (clientId.HasValue)
        {
            query = query.Where(x => x.ClientId == clientId.Value);
        }

        return query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }
}

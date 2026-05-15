using ConstructionManagement.DAL.Data;
using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.DAL.Repositories;

public class SiteRepository : GenericRepository<Site>, ISiteRepository
{
    private readonly AppDbContext _context;

    public SiteRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<Site?> GetByIdWithDetailsAsync(Guid id) =>
        _context.Sites
            .Include(x => x.Project)
                .ThenInclude(p => p!.Client)
            .Include(x => x.AssignedEngineer)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && !x.Project!.IsDeleted);

    public Task<List<Site>> SearchActiveAsync(string? search, string? status, Guid? projectId)
    {
        var query = _context.Sites
            .Include(x => x.Project)
                .ThenInclude(p => p!.Client)
            .Include(x => x.AssignedEngineer)
            .Where(x => !x.IsDeleted && !x.Project!.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(x =>
                x.SiteName.ToLower().Contains(s) ||
                x.Location.ToLower().Contains(s) ||
                (x.Description != null && x.Description.ToLower().Contains(s)) ||
                x.Project!.ProjectName.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim());
        }

        if (projectId.HasValue)
        {
            query = query.Where(x => x.ProjectId == projectId.Value);
        }

        return query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }
}

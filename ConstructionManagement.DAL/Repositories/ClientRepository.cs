using ConstructionManagement.DAL.Data;
using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.DAL.Repositories;

public class ClientRepository : GenericRepository<Client>, IClientRepository
{
    private readonly AppDbContext _context;

    public ClientRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<bool> EmailExistsAsync(string normalizedEmail, Guid? excludingId = null) =>
        _context.Clients.AnyAsync(x =>
            !x.IsDeleted &&
            x.Email == normalizedEmail &&
            (!excludingId.HasValue || x.Id != excludingId.Value));

    public Task<Client?> GetActiveByIdAsync(Guid id) =>
        _context.Clients
            .Include(x => x.Projects)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

    public Task<List<Client>> GetAllActiveAsync(string? search, bool? isActive)
    {
        var query = _context.Clients
            .Include(x => x.Projects)
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(s) ||
                x.Email.ToLower().Contains(s) ||
                x.Company.ToLower().Contains(s) ||
                x.Phone.ToLower().Contains(s));
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        return query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }
}

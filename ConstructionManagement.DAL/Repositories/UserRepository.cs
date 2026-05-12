using ConstructionManagement.DAL.Data;
using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.DAL.Repositories;

public class UserRepository : GenericRepository<AppUser>, IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
        : base(context)
    {
        _context = context;
    }

    public Task<bool> EmailExistsAsync(string normalizedEmail) =>
        _context.Users.AnyAsync(x => x.Email == normalizedEmail);

    public Task<AppUser?> GetByEmailAsync(string normalizedEmail) =>
        _context.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);

    public Task<AppUser?> GetByPasswordSetupTokenHashAsync(string tokenHash) =>
        _context.Users.FirstOrDefaultAsync(x =>
            !x.IsDeleted &&
            x.PasswordSetupTokenHash == tokenHash &&
            x.PasswordSetupTokenExpiresAtUtc != null);

    public Task<List<AppUser>> GetAllActiveAsync() =>
        _context.Users
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public Task<AppUser?> GetByIdActiveAsync(Guid id) =>
        _context.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

    public Task<AppUser?> GetByIdIncludingDeletedAsync(Guid id) =>
        _context.Users.FirstOrDefaultAsync(x => x.Id == id);

    public Task<int> CountActiveUsersAsync() =>
        _context.Users.CountAsync(x => !x.IsDeleted && x.IsActive);

    public Task<int> CountInactiveUsersAsync() =>
        _context.Users.CountAsync(x => !x.IsDeleted && !x.IsActive);

    public Task<int> CountByRoleAsync(string role) =>
        _context.Users.CountAsync(x => !x.IsDeleted && x.Role == role);

    public Task<List<AppUser>> SearchAsync(string? search, string? role, bool? isActive)
    {
        var query = _context.Users.Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(x =>
                x.FullName.ToLower().Contains(s) ||
                x.Email.ToLower().Contains(s) ||
                (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(x => x.Role == role);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        return query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

}

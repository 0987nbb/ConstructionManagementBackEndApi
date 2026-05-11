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

}

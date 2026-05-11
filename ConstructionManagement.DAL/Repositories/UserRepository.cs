using ConstructionManagement.DAL.Data;
using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.DAL.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> EmailExistsAsync(string normalizedEmail) =>
        _context.Users.AnyAsync(x => x.Email == normalizedEmail);

    public async Task AddAsync(AppUser user)
    {
        await _context.Users.AddAsync(user);
    }

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

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}

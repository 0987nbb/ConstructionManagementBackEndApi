using ConstructionManagement.DAL.Data;
using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.DAL.Repositories;

public class PasswordResetTokenRepository : GenericRepository<PasswordResetToken>, IPasswordResetTokenRepository
{
    private readonly AppDbContext _context;

    public PasswordResetTokenRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<PasswordResetToken?> GetValidByTokenHashAsync(string tokenHash, DateTime nowUtc) =>
        _context.PasswordResetTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash && !x.IsRevoked && x.UsedAtUtc == null && x.ExpiresAtUtc > nowUtc);

    public async Task RevokeAllActiveForUserAsync(Guid userId, DateTime nowUtc)
    {
        var tokens = await _context.PasswordResetTokens
            .Where(x => x.UserId == userId && !x.IsRevoked && x.UsedAtUtc == null && x.ExpiresAtUtc > nowUtc)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }
    }
}

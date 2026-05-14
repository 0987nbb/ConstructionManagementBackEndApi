using ConstructionManagement.DAL.Data;
using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.DAL.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash) =>
        _context.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

    public async Task RevokeAllActiveForUserAsync(Guid userId, DateTime revokedAtUtc, string? replacedByHash = null)
    {
        var tokens = await _context.RefreshTokens
            .Where(x => x.UserId == userId && !x.IsRevoked && x.ExpiresAtUtc > revokedAtUtc)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAtUtc = revokedAtUtc;
            if (!string.IsNullOrWhiteSpace(replacedByHash))
            {
                token.ReplacedByTokenHash = replacedByHash;
            }
        }
    }
}

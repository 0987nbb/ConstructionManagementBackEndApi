using ConstructionManagement.DAL.Data;
using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.API.Seed;

public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var email = configuration["SeedAdmin:Email"]?.Trim().ToLowerInvariant();
        var password = configuration["SeedAdmin:Password"];
        var fullName = configuration["SeedAdmin:FullName"]?.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
        {
            return;
        }

        var existing = await db.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (existing != null)
        {
            if (existing.Role != ApplicationRoles.Admin || existing.IsDeleted || !existing.IsActive)
            {
                existing.Role = ApplicationRoles.Admin;
                existing.IsDeleted = false;
                existing.IsActive = true;
                existing.MustChangePassword = false;
                existing.PasswordSetupTokenHash = null;
                existing.PasswordSetupTokenExpiresAtUtc = null;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.DeletedAt = null;
                await db.SaveChangesAsync();
            }
            return;
        }

        var admin = new AppUser
        {
            FullName = fullName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12),
            Role = ApplicationRoles.Admin,
            IsActive = true,
            IsDeleted = false,
            MustChangePassword = false,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }
}

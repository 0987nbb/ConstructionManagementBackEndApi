using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.DAL.Data;

public static class SeedData
{
    public static void SeedDatabase(AppDbContext context, SeedAdminOptions adminOptions)
    {
        context.Database.Migrate();

        EnsureAdmin(context, adminOptions);
    }

    private static void EnsureAdmin(AppDbContext context, SeedAdminOptions options)
    {
        if (!options.Enabled)
            return;

        var email = options.Email?.Trim().ToLowerInvariant();
        var password = options.Password;
        var fullName = options.FullName?.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
            return;

        var existing = context.Users.FirstOrDefault(x => x.Email == email);
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
                context.SaveChanges();
            }

            return;
        }

        context.Users.Add(new AppUser
        {
            FullName = fullName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12),
            Role = ApplicationRoles.Admin,
            IsActive = true,
            IsDeleted = false,
            MustChangePassword = false,
            IsFirstLogin = true,
            CreatedAt = DateTime.UtcNow
        });

        context.SaveChanges();
    }
}

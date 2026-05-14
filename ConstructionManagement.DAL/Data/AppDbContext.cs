using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasIndex(x => x.Email).IsUnique().HasFilter("[IsDeleted] = 0");
                entity.Property(x => x.FullName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
                entity.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
                entity.Property(x => x.Role).HasMaxLength(50).IsRequired();
                entity.Property(x => x.PhoneNumber).HasMaxLength(20);
                entity.Property(x => x.CreatedAt).IsRequired();
                entity.Property(x => x.IsActive).IsRequired();
                entity.Property(x => x.IsDeleted).IsRequired();
                entity.Property(x => x.MustChangePassword).IsRequired();
                entity.Property(x => x.IsFirstLogin).IsRequired();
                entity.Property(x => x.PasswordSetupTokenHash).HasMaxLength(128);

                entity.ToTable(t =>
                    t.HasCheckConstraint("CK_AppUsers_Role",
                        $"[Role] IN ('{ApplicationRoles.Admin}', '{ApplicationRoles.ProjectManager}', '{ApplicationRoles.Engineer}', '{ApplicationRoles.Accountant}', '{ApplicationRoles.Client}')"));
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasIndex(x => x.Email).IsUnique().HasFilter("[IsDeleted] = 0");
                entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
                entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
                entity.Property(x => x.Phone).HasMaxLength(30).IsRequired();
                entity.Property(x => x.Address).HasMaxLength(300).IsRequired();
                entity.Property(x => x.Company).HasMaxLength(160).IsRequired();
                entity.Property(x => x.IsActive).IsRequired();
                entity.Property(x => x.IsDeleted).IsRequired();
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasIndex(x => x.ProjectName);
                entity.HasIndex(x => x.ClientId);
                entity.HasIndex(x => x.AssignedEngineerId);
                entity.Property(x => x.ProjectName).HasMaxLength(160).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(1000);
                entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
                entity.Property(x => x.Budget).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(x => x.SpentAmount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(x => x.ProgressPercentage).IsRequired();
                entity.Property(x => x.IsDeleted).IsRequired();
                entity.HasOne(x => x.Client)
                    .WithMany(c => c.Projects)
                    .HasForeignKey(x => x.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.AssignedEngineer)
                    .WithMany()
                    .HasForeignKey(x => x.AssignedEngineerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Projects_Status",
                        "[Status] IN ('Planning', 'InProgress', 'OnHold', 'Completed', 'Cancelled')");
                    t.HasCheckConstraint("CK_Projects_ProgressPercentage",
                        "[ProgressPercentage] >= 0 AND [ProgressPercentage] <= 100");
                    t.HasCheckConstraint("CK_Projects_Budget", "[Budget] >= 0");
                    t.HasCheckConstraint("CK_Projects_SpentAmount", "[SpentAmount] >= 0");
                });
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(x => x.TokenHash).IsUnique();
                entity.HasIndex(x => new { x.UserId, x.IsRevoked, x.ExpiresAtUtc });
                entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
                entity.Property(x => x.CreatedByIp).HasMaxLength(64);
                entity.Property(x => x.ReplacedByTokenHash).HasMaxLength(128);
                entity.HasOne(x => x.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasIndex(x => x.TokenHash).IsUnique();
                entity.HasIndex(x => new { x.UserId, x.IsRevoked, x.ExpiresAtUtc });
                entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
                entity.HasOne(x => x.User)
                    .WithMany(u => u.PasswordResetTokens)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(x => new { x.UserId, x.TimestampUtc });
                entity.Property(x => x.Action).HasMaxLength(120).IsRequired();
                entity.Property(x => x.IpAddress).HasMaxLength(64);
                entity.Property(x => x.Metadata).HasMaxLength(1000);
                entity.HasOne(x => x.User)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}

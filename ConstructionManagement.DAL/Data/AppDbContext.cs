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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasIndex(x => x.Email).IsUnique();
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
                entity.HasIndex(x => x.Email).IsUnique();
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
                entity.HasIndex(x => x.Code).IsUnique();
                entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
                entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
                entity.HasOne(x => x.Client)
                    .WithMany(c => c.Projects)
                    .HasForeignKey(x => x.ClientId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}

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
                entity.Property(x => x.CreatedAt).IsRequired();

                entity.ToTable(t =>
                    t.HasCheckConstraint("CK_AppUsers_Role",
                        $"[Role] IN ('{ApplicationRoles.Admin}', '{ApplicationRoles.ProjectManager}', '{ApplicationRoles.Engineer}', '{ApplicationRoles.Accountant}', '{ApplicationRoles.Client}')"));
            });
        }
    }
}

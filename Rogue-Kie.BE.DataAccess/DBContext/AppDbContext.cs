using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.DataAccess.Models;
using Rogue_Kie.BE.DataAccess.SeedData;

namespace Rogue_Kie.BE.DataAccess.DBContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles => Set<Role>();

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Role mapping
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Name).IsUnique();

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.Description)
                    .HasMaxLength(255);
            });

            // User mapping
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Username).IsUnique();

                entity.Property(x => x.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.Password)
                    .IsRequired()
                    .HasMaxLength(255);

                // Relationship: User has one Role
                entity.HasOne(x => x.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Seed data
            modelBuilder.SeedRoles();
            modelBuilder.SeedUsers();
        }
    }
}
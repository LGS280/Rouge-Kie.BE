using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.DataAccess.Models;

namespace Rogue_Kie.BE.DataAccess.SeedData
{
    public static class DataSeed
    {
        public static void SeedRoles(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", Description = "Administrator - Full access" },
                new Role { Id = 2, Name = "Developer", Description = "Developer - Development access" },
                new Role { Id = 3, Name = "Player", Description = "Player - Regular user after login" },
                new Role { Id = 4, Name = "Guest", Description = "Guest - User without login" }
            );
        }

        public static void SeedUsers(this ModelBuilder modelBuilder)
        {
            // Admin account - Password: Admin@123
            var adminPassword = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            // Guest account - Password: Guest@123
            var guestPassword = BCrypt.Net.BCrypt.HashPassword("Guest@123");

            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 1, 
                    Username = "admin", 
                    Password = adminPassword,
                    RoleId = 1 
                },
                new User 
                { 
                    Id = 2, 
                    Username = "guest", 
                    Password = guestPassword,
                    RoleId = 4 
                }
            );
        }
    }
}

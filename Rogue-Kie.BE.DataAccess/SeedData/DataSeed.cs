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
                    Email = "admin@rogue-kie.local",
                    Password = adminPassword,
                    RoleId = 1,
                    CreatedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                },
                new User 
                { 
                    Id = 2, 
                    Username = "guest",
                    Email = "guest@rogue-kie.local",
                    Password = guestPassword,
                    RoleId = 4,
                    CreatedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                }
            );
        }

        public static void SeedGameConfigs(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EnemyConfig>().HasData(
                new EnemyConfig { Id = 1, EnemyName = "Slime", BaseHealth = 50, MoveSpeed = 2.0f, BaseDamage = 10, AttackSpeed = 1.5f, PrefabName = "SlimePrefab", CreatedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc) },
                new EnemyConfig { Id = 2, EnemyName = "Goblin", BaseHealth = 100, MoveSpeed = 3.5f, BaseDamage = 15, AttackSpeed = 1.2f, PrefabName = "GoblinPrefab", CreatedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc) },
                new EnemyConfig { Id = 3, EnemyName = "Orc", BaseHealth = 250, MoveSpeed = 1.5f, BaseDamage = 30, AttackSpeed = 2.0f, PrefabName = "OrcPrefab", CreatedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc) },
                new EnemyConfig { Id = 4, EnemyName = "Dragon", BaseHealth = 1000, MoveSpeed = 5.0f, BaseDamage = 100, AttackSpeed = 0.5f, PrefabName = "DragonPrefab", CreatedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            modelBuilder.Entity<BulletConfig>().HasData(
                new BulletConfig { Id = 1, BulletName = "Basic Bullet", Damage = 20, CritRate = 0.1f, FlightSpeed = 10.0f, PiercingCount = 0, PrefabName = "BasicBulletPrefab" },
                new BulletConfig { Id = 2, BulletName = "Buckshot", Damage = 10, CritRate = 0.05f, FlightSpeed = 15.0f, PiercingCount = 0, PrefabName = "BuckshotPrefab" },
                new BulletConfig { Id = 3, BulletName = "Sniper Round", Damage = 150, CritRate = 0.5f, FlightSpeed = 30.0f, PiercingCount = 3, PrefabName = "SniperRoundPrefab" },
                new BulletConfig { Id = 4, BulletName = "Rifle Bullet", Damage = 30, CritRate = 0.2f, FlightSpeed = 20.0f, PiercingCount = 1, PrefabName = "RifleBulletPrefab" },
                new BulletConfig { Id = 5, BulletName = "Sword Slash", Damage = 40, CritRate = 0.3f, FlightSpeed = 5.0f, PiercingCount = 99, PrefabName = "SwordSlashPrefab" }
            );

            modelBuilder.Entity<WeaponConfig>().HasData(
                new WeaponConfig { Id = 1, WeaponName = "Pistol", PrefabName = "PistolPrefab", ShootSound = "pistol_shoot", ShootVolume = 1.0f, HandPositionX = 0.1f, HandPositionY = 0.05f, HandPositionZ = 0f, RecoilDistance = 0.1f, RecoilDuration = 0.05f, ReturnDuration = 0.1f, FireRate = 0.5f, ManaCost = 0, BulletsPerShot = 1, SpreadAngle = 2.0f, BulletId = 1 },
                new WeaponConfig { Id = 2, WeaponName = "Shotgun", PrefabName = "ShotgunPrefab", ShootSound = "shotgun_shoot", ShootVolume = 1.2f, HandPositionX = 0.2f, HandPositionY = 0.1f, HandPositionZ = 0f, RecoilDistance = 0.3f, RecoilDuration = 0.1f, ReturnDuration = 0.2f, FireRate = 1.5f, ManaCost = 2, BulletsPerShot = 5, SpreadAngle = 15.0f, BulletId = 2 },
                new WeaponConfig { Id = 3, WeaponName = "Sniper", PrefabName = "SniperPrefab", ShootSound = "sniper_shoot", ShootVolume = 1.5f, HandPositionX = 0.3f, HandPositionY = 0.15f, HandPositionZ = 0f, RecoilDistance = 0.5f, RecoilDuration = 0.15f, ReturnDuration = 0.3f, FireRate = 2.0f, ManaCost = 5, BulletsPerShot = 1, SpreadAngle = 0.0f, BulletId = 3 },
                new WeaponConfig { Id = 4, WeaponName = "Assault Rifle", PrefabName = "RiflePrefab", ShootSound = "rifle_shoot", ShootVolume = 0.8f, HandPositionX = 0.2f, HandPositionY = 0.1f, HandPositionZ = 0f, RecoilDistance = 0.15f, RecoilDuration = 0.05f, ReturnDuration = 0.1f, FireRate = 0.2f, ManaCost = 1, BulletsPerShot = 1, SpreadAngle = 5.0f, BulletId = 4 },
                new WeaponConfig { Id = 5, WeaponName = "Sword", PrefabName = "SwordPrefab", ShootSound = "sword_swing", ShootVolume = 1.0f, HandPositionX = 0.1f, HandPositionY = 0.2f, HandPositionZ = 0f, RecoilDistance = 0f, RecoilDuration = 0f, ReturnDuration = 0f, FireRate = 0.8f, ManaCost = 0, BulletsPerShot = 1, SpreadAngle = 30.0f, BulletId = 5 }
            );

            modelBuilder.Entity<LevelConfig>().HasData(
                new LevelConfig { Id = 1, StageId = 1, FloorNumber = 1, DifficultyMultiplier = 1.0f },
                new LevelConfig { Id = 2, StageId = 1, FloorNumber = 2, DifficultyMultiplier = 1.2f },
                new LevelConfig { Id = 3, StageId = 1, FloorNumber = 3, DifficultyMultiplier = 1.5f },
                new LevelConfig { Id = 4, StageId = 1, FloorNumber = 4, DifficultyMultiplier = 2.0f },
                new LevelConfig { Id = 5, StageId = 1, FloorNumber = 5, DifficultyMultiplier = 3.0f } // Boss stage
            );

            modelBuilder.Entity<BuffConfig>().HasData(
                new BuffConfig { Id = 1, BuffName = "Speed Boost", Value = 1.5f, Description = "Increases movement speed.", IconPath = "speed_icon", BuffType = "StatModifier", Rarity = "Common" },
                new BuffConfig { Id = 2, BuffName = "Damage Boost", Value = 2.0f, Description = "Increases damage dealt.", IconPath = "damage_icon", BuffType = "StatModifier", Rarity = "Rare" },
                new BuffConfig { Id = 3, BuffName = "Health Regen", Value = 10.0f, Description = "Regenerates health over time.", IconPath = "regen_icon", BuffType = "Utility", Rarity = "Epic" }
            );
        }
    }
}

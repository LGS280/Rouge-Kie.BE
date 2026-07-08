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


        public DbSet<EnemyConfig> EnemyConfigs { get; set; }
        public DbSet<BulletConfig> BulletConfigs { get; set; }
        public DbSet<WeaponConfig> WeaponConfigs { get; set; }
        public DbSet<LevelConfig> LevelConfigs { get; set; }
        public DbSet<BuffConfig> BuffConfigs { get; set; }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<PlayerProfile> PlayerProfiles => Set<PlayerProfile>();
        public DbSet<Leaderboard> Leaderboards => Set<Leaderboard>();
        public DbSet<Character> Characters => Set<Character>();
        public DbSet<PlayerCharacter> PlayerCharacters => Set<PlayerCharacter>();
        public DbSet<CosmeticItem> CosmeticItems => Set<CosmeticItem>();
        public DbSet<PlayerCosmetic> PlayerCosmetics => Set<PlayerCosmetic>();
        public DbSet<PlayerWeapon> PlayerWeapons => Set<PlayerWeapon>();
        public DbSet<RoomConfig> RoomConfigs { get; set; }
        public DbSet<RoomWaveConfig> RoomWaveConfigs { get; set; }
        public DbSet<WaveEnemyDetail> WaveEnemyDetails { get; set; }
        public DbSet<ShopItem> ShopItems => Set<ShopItem>();
        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<GameSession> GameSessions => Set<GameSession>();
        public DbSet<SessionPlayer> SessionPlayers => Set<SessionPlayer>();
        public DbSet<RunHistory> RunHistories => Set<RunHistory>();

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
                entity.HasIndex(x => x.Email).IsUnique();

                entity.Property(x => x.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.Email)
                    .IsRequired()
                    .HasMaxLength(255);

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
            modelBuilder.SeedGameConfigs();
            modelBuilder.SeedCharacters();

            // Game Configs mapping
            modelBuilder.Entity<EnemyConfig>().ToTable("EnemyConfigs");
            modelBuilder.Entity<BulletConfig>().ToTable("BulletConfigs");
            modelBuilder.Entity<WeaponConfig>().ToTable("WeaponConfigs");
            modelBuilder.Entity<LevelConfig>().ToTable("LevelConfigs");
            modelBuilder.Entity<BuffConfig>().ToTable("BuffConfigs");

            // New mappings
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");
                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PlayerProfile>(entity =>
            {
                entity.ToTable("PlayerProfiles");
                entity.HasIndex(x => x.UserId).IsUnique();
                entity.HasOne(x => x.User)
                    .WithOne()
                    .HasForeignKey<PlayerProfile>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Leaderboard>(entity =>
            {
                entity.ToTable("Leaderboards");
                entity.HasIndex(x => x.UserId).IsUnique();
                entity.HasOne(x => x.User)
                    .WithOne()
                    .HasForeignKey<Leaderboard>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Character>().ToTable("Characters");

            modelBuilder.Entity<PlayerCharacter>(entity =>
            {
                entity.ToTable("PlayerCharacters");
                entity.HasOne(x => x.PlayerProfile)
                    .WithMany()
                    .HasForeignKey(x => x.ProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Character)
                    .WithMany()
                    .HasForeignKey(x => x.CharacterId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CosmeticItem>().ToTable("CosmeticItems");

            modelBuilder.Entity<PlayerCosmetic>(entity =>
            {
                entity.ToTable("PlayerCosmetics");
                entity.HasOne(x => x.PlayerProfile)
                    .WithMany()
                    .HasForeignKey(x => x.ProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.CosmeticItem)
                    .WithMany()
                    .HasForeignKey(x => x.CosmeticId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PlayerWeapon>(entity =>
            {
                entity.ToTable("PlayerWeapons");
                entity.HasOne(x => x.PlayerProfile)
                    .WithMany()
                    .HasForeignKey(x => x.ProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.WeaponConfig)
                    .WithMany()
                    .HasForeignKey(x => x.WeaponConfigId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RoomConfig>(entity =>
            {
                entity.ToTable("RoomConfigs");
                entity.HasOne(x => x.LevelConfig)
                    .WithMany()
                    .HasForeignKey(x => x.LevelConfigId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RoomWaveConfig>(entity =>
            {
                entity.ToTable("RoomWaveConfigs");
                entity.HasOne(x => x.RoomConfig)
                    .WithMany()
                    .HasForeignKey(x => x.RoomConfigId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WaveEnemyDetail>(entity =>
            {
                entity.ToTable("WaveEnemyDetails");
                entity.HasOne(x => x.RoomWaveConfig)
                    .WithMany()
                    .HasForeignKey(x => x.RoomWaveConfigId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.EnemyConfig)
                    .WithMany()
                    .HasForeignKey(x => x.EnemyConfigId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ShopItem>().ToTable("ShopItems");

            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.ToTable("Inventory");
                entity.HasOne(x => x.PlayerProfile)
                    .WithMany()
                    .HasForeignKey(x => x.ProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.ShopItem)
                    .WithMany()
                    .HasForeignKey(x => x.ShopItemId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transactions");
                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.ShopItem)
                    .WithMany()
                    .HasForeignKey(x => x.ShopItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GameSession>(entity =>
            {
                entity.ToTable("GameSessions");
                entity.HasOne(x => x.HostUser)
                    .WithMany()
                    .HasForeignKey(x => x.HostUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SessionPlayer>(entity =>
            {
                entity.ToTable("SessionPlayers");
                entity.HasOne(x => x.GameSession)
                    .WithMany()
                    .HasForeignKey(x => x.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Character)
                    .WithMany()
                    .HasForeignKey(x => x.CharacterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RunHistory>(entity =>
            {
                entity.ToTable("RunHistories");
                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.GameSession)
                    .WithMany()
                    .HasForeignKey(x => x.SessionId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Character)
                    .WithMany()
                    .HasForeignKey(x => x.CharacterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
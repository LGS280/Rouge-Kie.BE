using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Rogue_Kie.BE.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddGameConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuffConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BuffName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IconPath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    BuffType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Value = table.Column<float>(type: "real", nullable: false),
                    Rarity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuffConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnemyConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnemyName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BaseHealth = table.Column<int>(type: "integer", nullable: false),
                    BaseDamage = table.Column<int>(type: "integer", nullable: false),
                    MoveSpeed = table.Column<float>(type: "real", nullable: false),
                    AttackSpeed = table.Column<float>(type: "real", nullable: false),
                    PrefabName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnemyConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LevelConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FloorNumber = table.Column<int>(type: "integer", nullable: false),
                    DifficultyMultiplier = table.Column<float>(type: "real", nullable: false),
                    MaxEnemiesToSpawn = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeaponConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WeaponName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Damage = table.Column<int>(type: "integer", nullable: false),
                    FireRate = table.Column<float>(type: "real", nullable: false),
                    AmmoCapacity = table.Column<int>(type: "integer", nullable: false),
                    ReloadTime = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeaponConfigs", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$rVnusOq/UhPJSJ118uvgbeUSMTJMP1Ftpnz3OQLCZCckmUgh0v3Y6");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$2BFqDxj09IVDhmDLjclCU.eCXZK5/IcsasrnT/LecuLoqw/P8I67G");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuffConfigs");

            migrationBuilder.DropTable(
                name: "EnemyConfigs");

            migrationBuilder.DropTable(
                name: "LevelConfigs");

            migrationBuilder.DropTable(
                name: "WeaponConfigs");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$D9y9ZT8s29sfFCdJYc9AC.4DnvXil2rYuqssosqd/yYjOEfs9r51m");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$m74bFLGzrVsjXDpD400Uf.HJ6blUGXKAPpaivMX2TmLjGJb6YnCKS");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Rogue_Kie.BE.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedGameConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BuffConfigs",
                columns: new[] { "Id", "BuffName", "BuffType", "Description", "IconPath", "Rarity", "Value" },
                values: new object[,]
                {
                    { 1, "Speed Boost", "StatModifier", "Increases movement speed.", "speed_icon", "Common", 1.5f },
                    { 2, "Damage Boost", "StatModifier", "Increases damage dealt.", "damage_icon", "Rare", 2f },
                    { 3, "Health Regen", "Utility", "Regenerates health over time.", "regen_icon", "Epic", 10f }
                });

            migrationBuilder.InsertData(
                table: "EnemyConfigs",
                columns: new[] { "Id", "AttackSpeed", "BaseDamage", "BaseHealth", "EnemyName", "MoveSpeed", "PrefabName" },
                values: new object[,]
                {
                    { 1, 1.5f, 10, 50, "Slime", 2f, "SlimePrefab" },
                    { 2, 1.2f, 15, 100, "Goblin", 3.5f, "GoblinPrefab" },
                    { 3, 2f, 30, 250, "Orc", 1.5f, "OrcPrefab" },
                    { 4, 0.5f, 100, 1000, "Dragon", 5f, "DragonPrefab" }
                });

            migrationBuilder.InsertData(
                table: "LevelConfigs",
                columns: new[] { "Id", "DifficultyMultiplier", "FloorNumber", "MaxEnemiesToSpawn" },
                values: new object[,]
                {
                    { 1, 1f, 1, 10 },
                    { 2, 1.2f, 2, 15 },
                    { 3, 1.5f, 3, 25 },
                    { 4, 2f, 4, 40 },
                    { 5, 3f, 5, 1 }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$C24MGKhdP9qoPh3GdRNVvO4rUsY5REvy8J/S4C9qrYZ.7ZYf1Q3Si");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$98t77WPOjsGRv69nkgb7NeBJHJsiSJ2EzmvqaImddhoMWPa54Eqe2");

            migrationBuilder.InsertData(
                table: "WeaponConfigs",
                columns: new[] { "Id", "Damage", "FireRate", "WeaponName" },
                values: new object[,]
                {
                    { 1, 20, 0.5f, "Pistol" },
                    { 2, 50, 1.5f, "Shotgun" },
                    { 3, 150, 2f, "Sniper" },
                    { 4, 30, 0.2f, "Assault Rifle" },
                    { 5, 40, 0.8f, "Sword" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BuffConfigs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BuffConfigs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "BuffConfigs",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "EnemyConfigs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EnemyConfigs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "EnemyConfigs",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "EnemyConfigs",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "LevelConfigs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "LevelConfigs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "LevelConfigs",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "LevelConfigs",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "LevelConfigs",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$HmaXUDRQHiB1VCRbDlE3VeawNChGPVzAsLU.0vbHNlaVcR/5tb61m");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$H9DVNYu5cp/vVHSaXUw0ju/EYB1yfQkrNPksfJAvxiSFKqttWfvAS");
        }
    }
}

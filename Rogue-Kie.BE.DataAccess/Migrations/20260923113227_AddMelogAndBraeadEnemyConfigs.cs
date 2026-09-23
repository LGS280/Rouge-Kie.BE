using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Rogue_Kie.BE.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddMelogAndBraeadEnemyConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "EnemyConfigs",
                columns: new[] { "Id", "AttackSpeed", "BaseHealth", "CreatedAt", "EnemyName", "MoveSpeed", "PrefabName" },
                values: new object[,]
                {
                    { 5, 3f, 500, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Melog", 2.8f, "Melog" },
                    { 6, 3f, 1800, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Braead", 2.2f, "Braead" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EnemyConfigs",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "EnemyConfigs",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}

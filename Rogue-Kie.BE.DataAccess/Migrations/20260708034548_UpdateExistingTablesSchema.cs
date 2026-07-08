using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Rogue_Kie.BE.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExistingTablesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrationOtps");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "LevelConfigs",
                newName: "LevelConfigId");

            migrationBuilder.RenameColumn(
                name: "MaxEnemiesToSpawn",
                table: "LevelConfigs",
                newName: "StageId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLogin",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "EnemyConfigs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "EnemyConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "EnemyConfigs",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "EnemyConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "EnemyConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "LevelConfigId",
                keyValue: 1,
                column: "StageId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "LevelConfigId",
                keyValue: 2,
                column: "StageId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "LevelConfigId",
                keyValue: 3,
                column: "StageId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "LevelConfigId",
                keyValue: 4,
                column: "StageId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "IsActive", "LastLogin", "Password" },
                values: new object[] { new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "$2a$11$XvzDRR5slzvRSaXyiU9XSuf1jQST.ienOHTpBa5wBb6Z4Oxw5q4ae" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "IsActive", "LastLogin", "Password" },
                values: new object[] { new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "$2a$11$JR24rcgcbVZeFgPTVFaFZOZ3JPKV1y42R9GGFz3mZtwHbjnzCSjTO" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLogin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "EnemyConfigs");

            migrationBuilder.RenameColumn(
                name: "LevelConfigId",
                table: "LevelConfigs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "StageId",
                table: "LevelConfigs",
                newName: "MaxEnemiesToSpawn");

            migrationBuilder.CreateTable(
                name: "RegistrationOtps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OtpCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationOtps", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "MaxEnemiesToSpawn",
                value: 10);

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "Id",
                keyValue: 2,
                column: "MaxEnemiesToSpawn",
                value: 15);

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "MaxEnemiesToSpawn",
                value: 25);

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "MaxEnemiesToSpawn",
                value: 40);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$beR2aSvrBiAQYucwhWlAR.SqgRuCV4P3p4PoPpA7Z61LTuSP8KOlu");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$nw2coeLSJWL2S/MwiTdQZOJcFrjgtIdoQenLXW6GVFpwFB/35xjA2");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationOtps_Email",
                table: "RegistrationOtps",
                column: "Email");
        }
    }
}
